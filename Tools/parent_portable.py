"""Parent-page portable export and read-only inspection, using the proven format."""
from contextlib import contextmanager
import base64
import binascii
import hmac
import threading
import uuid

import portable_recovery as portable
from parent_server import OperationError
from server_recovery import Recovery, check, file_bytes
from shared_garden_runtime import ROOT

UPLOAD_LIMIT = ((portable.FILE_LIMIT + 2) // 3) * 4
REQUEST_LIMIT = UPLOAD_LIMIT + 16384
# The password KDF intentionally uses substantial memory. Refuse competing
# requests across all panels in this helper, rather than queuing secret bodies.
_slot = threading.Lock()


@contextmanager
def one_operation():
    if not _slot.acquire(blocking=False):
        raise OperationError('Another protected-backup operation is running. Try again when it finishes.')
    try:
        yield
    finally:
        _slot.release()


class ParentPortable:
    def __init__(self, controller, remember_backup):
        self.controller = controller
        self.remember_backup = remember_backup

    def export(self, data):
        check(set(data) == {'password', 'confirmation'}, 'Invalid protected-backup request.')
        password, confirmation = data['password'], data['confirmation']
        portable.password_bytes(password); portable.password_bytes(confirmation)
        check(hmac.compare_digest(password.encode(), confirmation.encode()),
              'The passphrases do not match. No protected copy was created.')
        with one_operation():
            backup = Recovery(self.controller.family, self.controller.build).backup()
            destination = ROOT / 'LocalData/ServerBackups' / (uuid.uuid4().hex + '.lwportable')
            portable.export(backup['path'], destination, password)
            self.remember_backup(backup)
            # A retained local encrypted copy makes an interrupted download
            # harmless. No source path, enrollment or password enters metadata.
            return file_bytes(destination, portable.FILE_LIMIT)

    def verify(self, data):
        check(set(data) == {'password', 'file'}, 'Invalid backup-check request.')
        portable.password_bytes(data['password'])
        check(isinstance(data['file'], str) and len(data['file']) <= UPLOAD_LIMIT,
              'Choose a supported protected-backup file.')
        try:
            raw = base64.b64decode(data['file'], validate=True)
        except (ValueError, binascii.Error):
            raise OperationError('Choose a supported protected-backup file.') from None
        with one_operation():
            result = portable.verify_bytes(raw, data['password'])
        check(result['world'] == self.controller.family,
              'This verified backup belongs to a different family. Nothing was restored.')
        # Only non-secret confirmation fields go back to the page.
        return dict(result='portable-verified', build=result['build'], revision=result['revision'],
                    profiles=result['profiles'], independentRecoveryQualified=False)
