'use strict';
const $ = id => document.getElementById(id);
const token = location.hash.slice(1) || sessionStorage.getItem('parentSession') || '';
if (location.hash) { sessionStorage.setItem('parentSession', token); history.replaceState(null, '', '/'); }
let latest = null, busy = false, polling = false;
async function api(path, body, binary=false) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), body ? 90000 : 16000);
  try {
    const response = await fetch('/api/' + path, { method: body ? 'POST' : 'GET', cache: 'no-store',
      headers: {'X-Little-Weeps': token, ...(body ? {'Content-Type': 'application/json'} : {})},
      body: body ? JSON.stringify(body) : undefined, signal: controller.signal });
    if (binary && response.ok) return await response.blob();
    const data = await response.json();
    if (!response.ok) throw new Error(data.error || 'The operation could not be verified.');
    return data;
  } finally { clearTimeout(timeout); }
}
function buttons() { $('start').disabled = busy || !latest?.canStart; $('stop').disabled = busy || !latest?.canStop; $('refresh').disabled = busy || polling;
  $('backup').disabled = busy || !latest?.canBackup;
  $('recovery-enable').disabled = busy || !latest?.recovery?.canEnable;
  $('recovery-pause').disabled = busy || !latest?.recovery?.canPause;
  $('startup-enable').disabled = busy || !latest?.startup?.canEnable;
  $('startup-disable').disabled = busy || !latest?.startup?.canDisable;
  $('portable-export').disabled = busy || !latest?.portable?.available || !latest?.canBackup;
  $('portable-verify').disabled = busy || !latest?.portable?.available;
}
function show(data) {
  latest = data;
  $('state').textContent = {ready:'Ready for play', stopped:'Server stopped', unreachable:'Status unavailable'}[data.state] || 'Checking status…';
  $('light').className = 'light ' + data.state;
  $('build').textContent = data.build ? 'Build ' + data.build : 'Family world';
  $('message').textContent = data.message;
  $('players').textContent = data.players === null ? '—' : data.players + ' / 4';
  $('save').textContent = data.save.state === 'verified' ? new Date(data.save.savedAt).toLocaleTimeString([], {hour:'numeric',minute:'2-digit',second:'2-digit'}) : data.save.state === 'missing' ? 'No save yet' : 'Needs attention';
  $('save-detail').textContent = data.save.state === 'verified' ? new Date(data.save.savedAt).toLocaleDateString() + ' · Saved file verified' : data.save.state === 'missing' ? 'The server will load or create this enrolled world.' : 'Save verification unavailable. No reset will be attempted.';
  $('checked').textContent = 'Checked ' + new Date(data.checkedAt).toLocaleTimeString() + ' · Refreshes automatically';
  $('backup-state').textContent = {none:'No backup yet',verified:'Verified',changed:'Needs attention',unavailable:'Unavailable'}[data.backup?.state] || 'Unavailable';
  $('backup-detail').textContent = data.backup?.state === 'verified' ? 'Saved ' + new Date(data.backup.verifiedAt).toLocaleString() + ' · On this PC' : 'Create a verified copy without interrupting play.';
  $('recovery-state').textContent = {off:'Off',healthy:'Watching',paused:'Paused after stop',waiting:'Waiting to retry',restarting:'Restarting',recovered:'Recovered',blocked:'Needs attention',checking:'Checking…','needs-attention':'Needs attention'}[data.recovery?.status] || 'Unavailable';
  $('recovery-detail').textContent = data.recovery?.message || 'Reopen the updated parent shortcut.';
  $('startup-state').textContent = {off:'Off',configured:'Shortcut configured',changed:'Needs attention',unavailable:'Unavailable'}[data.startup?.state] || 'Update needed';
  $('startup-detail').textContent = data.startup?.message || 'These controls need the updated parent helper. Your game can keep running.';
  $('portable-availability').textContent = data.portable?.available ? 'Passphrases are used only for this operation and are not saved by the control page.' : 'These controls need the updated parent helper. Your game can keep running.';
  buttons();
}
function feedback(text, error=false) { $('feedback').textContent = text; $('feedback').className = error ? 'error' : ''; }
async function refresh() {
  if (polling || busy) return;
  polling = true; buttons();
  try { show(await api('status')); }
  catch (error) { latest = null; $('state').textContent = 'Control page disconnected'; $('light').className = 'light unreachable'; $('players').textContent = '—'; $('message').textContent = 'The server may still be running. Reopen the parent shortcut to check.'; $('checked').textContent = 'Live status unavailable; saved time below is from the last successful check.'; feedback(error.message, true); }
  finally { polling = false; buttons(); }
}
async function action(kind) {
  if (busy || !latest || !(kind === 'start' ? latest.canStart : latest.canStop)) return;
  const instanceId = latest.instanceId;
  busy = true; buttons(); feedback(kind === 'start' ? 'Starting the enrolled family world…' : 'Checking for players and saving before stopping…');
  try { const result = await api(kind, {instanceId}); show(result.status); feedback({started:'Server ready. The kids can join automatically.', 'already-running':'The server is already running.', stopped:'Server stopped. The saved world is verified.'}[result.result] || 'Done.'); }
  catch (error) { feedback(error.message, true); }
  finally { busy = false; buttons(); await refresh(); }
}
$('start').addEventListener('click', () => action('start'));
$('stop').addEventListener('click', () => action('stop'));
$('refresh').addEventListener('click', refresh);
async function care(kind) {
  if (busy || !latest) return;
  busy = true; buttons(); feedback(kind === 'backup' ? 'Saving and verifying a separate backup…' : kind.startsWith('startup-') ? 'Updating the sign-in shortcut…' : 'Updating the recovery helper…');
  try { const result = await api(kind, {}); show(result.status); feedback({'backup-verified':'Backup verified and saved on this PC.','recovery-enabled':'Automatic crash recovery enabled.','recovery-paused':'Automatic recovery paused. Current players can keep playing.','startup-configured':'Sign-in shortcut verified. Test it at your next sign-in; saved Stop/Pause choices are preserved.','startup-removed':'Sign-in shortcut removed. Current play and recovery continue.'}[result.result] || 'Done.'); }
  catch (error) { feedback(error.message, true); }
  finally { busy = false; buttons(); await refresh(); }
}
for (const kind of ['backup','recovery-enable','recovery-pause','startup-enable','startup-disable']) $(kind).addEventListener('click', () => care(kind));
function clearPassphrases() {
  for (const id of ['export-password','export-confirmation','verify-password']) $(id).value = '';
}
function portableFeedback(text, error=false) {
  $('portable-feedback').textContent = text; $('portable-feedback').className = error ? 'error' : '';
}
async function protectedBackup(event, kind) {
  event.preventDefault();
  if (busy || !latest?.portable?.available) return;
  if (kind === 'export' && !latest.canBackup) return;
  busy = true; buttons(); portableFeedback(kind === 'export' ? 'Preparing and checking your protected copy…' : 'Checking the selected backup…');
  let body;
  try {
    if (kind === 'export') {
      body = {password:$('export-password').value, confirmation:$('export-confirmation').value};
      clearPassphrases();
      const blob = await api('portable-export', body, true);
      const url = URL.createObjectURL(blob), link = document.createElement('a');
      link.href = url; link.download = 'Little-Weeps-' + new Date().toISOString().replace(/[:.]/g,'-') + '.lwportable';
      document.body.appendChild(link); link.click(); link.remove();
      setTimeout(() => URL.revokeObjectURL(url), 60000);
      portableFeedback('Protected copy prepared; download requested. Check your browser downloads, move the file to separate storage, then check that copy here.');
    } else {
      const file = $('verify-file').files[0];
      body = {password:$('verify-password').value}; clearPassphrases();
      if (!file || !file.name.toLowerCase().endsWith('.lwportable') || file.size > latest.portable.maxBytes) throw new Error('Choose a supported .lwportable backup within the file size limit.');
      const bytes = new Uint8Array(await file.arrayBuffer());
      let encoded = '';
      for (let i=0;i<bytes.length;i+=32768) encoded += String.fromCharCode(...bytes.subarray(i,i+32768));
      body.file = btoa(encoded);
      const result = await api('portable-verify', body);
      portableFeedback('Backup verified for this family: build ' + result.build + ', saved revision ' + result.revision + ', ' + result.profiles + ' player profiles. Nothing was restored. Testing recovery on another computer is still needed.');
    }
  } catch (error) { portableFeedback(error.message, true); }
  finally { body = null; clearPassphrases(); busy = false; buttons(); await refresh(); }
}
$('portable-export-form').addEventListener('submit', event => protectedBackup(event,'export'));
$('portable-verify-form').addEventListener('submit', event => protectedBackup(event,'verify'));
document.addEventListener('visibilitychange', () => { if (document.hidden) clearPassphrases(); });
window.addEventListener('pagehide', clearPassphrases);
refresh(); setInterval(refresh, 3000);
