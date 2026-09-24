"""This project's build Mac route; a changed IP retains the enrolled SSH host identity."""
import ipaddress
import json
import os
from pathlib import Path

ROOT=Path(__file__).resolve().parent.parent
ALIAS='eduardos-mbp.lan'
record=ROOT/'LocalData/mac-connection.json'
address=ALIAS
if record.exists():
    # Local, ignored routing metadata only. Never accept shell/options/usernames
    # from the record, or replace the pinned host identity with a new IP identity.
    address=str(ipaddress.IPv4Address(json.loads(record.read_text(encoding='utf-8'))['address']))
HOST='nayster@'+address
SSH=Path(os.environ['WINDIR'])/'System32/OpenSSH/ssh.exe'
SCP=SSH.with_name('scp.exe')
OPTIONS=['-i',str(Path.home()/'.ssh/little_weeps_mac_ed25519'),'-o','HostKeyAlias='+ALIAS,
         '-o','StrictHostKeyChecking=yes','-o','IdentitiesOnly=yes','-o','BatchMode=yes','-o','ConnectTimeout=8']
