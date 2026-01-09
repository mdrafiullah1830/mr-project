// Dev view helper: toggles preview sizes for Desktop/Tablet/Mobile
(function(){
  const STORAGE_KEY = 'mrshop_devview';
  function createPanel(){
    const container = document.createElement('div');
    container.className = 'devview-toggle';
    container.innerHTML = `
      <div class="devview-panel" role="toolbar" aria-label="Dev view">
        <button class="devview-btn" data-view="desktop" title="Desktop">Desk</button>
        <button class="devview-btn" data-view="tablet" title="Tablet">Tab</button>
        <button class="devview-btn" data-view="mobile" title="Mobile">Mob</button>
        <button class="devview-close" aria-label="Close devview">✕</button>
      </div>`;
    document.body.appendChild(container);

    container.addEventListener('click', (e)=>{
      const btn = e.target.closest('.devview-btn');
      if (btn){ setView(btn.getAttribute('data-view')); }
      if (e.target.closest('.devview-close')){ container.style.display = 'none'; localStorage.setItem(STORAGE_KEY+'_hidden', '1'); }
    });

    return container;
  }

  function setView(view){
    document.documentElement.classList.remove('devview-desktop','devview-tablet','devview-mobile','devview-outline');
    if (view){
      document.documentElement.classList.add('devview-'+view,'devview-outline');
      localStorage.setItem(STORAGE_KEY, view);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
    updateButtons();
  }

  function updateButtons(){
    const view = localStorage.getItem(STORAGE_KEY);
    document.querySelectorAll('.devview-btn').forEach(b=>{
      b.classList.toggle('active', b.getAttribute('data-view')===view);
    });
  }

  // Initialize when DOM ready
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
  else init();

  function init(){
    const hidden = localStorage.getItem(STORAGE_KEY+'_hidden');
    const panel = createPanel();
    if (hidden) panel.style.display = 'none';
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) setView(saved);
    // keyboard shortcuts: D, T, M to toggle
    window.addEventListener('keydown', (e)=>{
      if (e.altKey && !e.ctrlKey && !e.metaKey){
        if (e.key.toLowerCase()==='d') setView('desktop');
        if (e.key.toLowerCase()==='t') setView('tablet');
        if (e.key.toLowerCase()==='m') setView('mobile');
        if (e.key === 'Escape') { document.querySelector('.devview-toggle').style.display='none'; localStorage.setItem(STORAGE_KEY+'_hidden','1'); }
      }
    });
  }
})();
