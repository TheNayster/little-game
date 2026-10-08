"""Run the isolated milestone8 live demo and retain owner-controlled windows."""
import importlib.util
import sys
from pathlib import Path

path=Path(__file__).resolve().parents[1]/'Verification/Test-ElephantSnack.py'
spec=importlib.util.spec_from_file_location('elephant_snack_review',path)
module=importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
if __name__=='__main__':
    sys.argv.append('--review')
    module.main()
