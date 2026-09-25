'use strict';
const $ = id => document.getElementById(id);
const token = location.hash.slice(1) || sessionStorage.getItem('parentSession') || '';
if (location.hash) { sessionStorage.setItem('parentSession', token); history.replaceState(null, '', '/'); }
let latest = null, busy = false, polling = false;
async function api(path, body) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), body ? 90000 : 16000);
  try {
    const response = await fetch('/api/' + path, { method: body ? 'POST' : 'GET', cache: 'no-store',
      headers: {'X-Little-Weeps': token, ...(body ? {'Content-Type': 'application/json'} : {})},
      body: body ? JSON.stringify(body) : undefined, signal: controller.signal });
    const data = await response.json();
    if (!response.ok) throw new Error(data.error || 'The operation could not be verified.');
    return data;
  } finally { clearTimeout(timeout); }
}
function buttons() { $('start').disabled = busy || !latest?.canStart; $('stop').disabled = busy || !latest?.canStop; $('refresh').disabled = busy || polling; }
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
refresh(); setInterval(refresh, 3000);
