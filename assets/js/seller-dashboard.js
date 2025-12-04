// Basic interactions for seller dashboard (placeholder logic)
document.addEventListener('DOMContentLoaded', ()=>{
  // Sidebar nav behavior
  document.querySelectorAll('.sd-nav a').forEach(a=>{
    a.addEventListener('click', (e)=>{
      e.preventDefault();
      // If user clicked Settings, navigate to separate settings page
      const target = a.dataset.target;
      if(target === 'settings'){
        window.location.href = 'seller-settings.html';
        return;
      }
      document.querySelectorAll('.sd-nav a').forEach(x=>x.classList.remove('active'));
      a.classList.add('active');
      document.querySelectorAll('.sd-section').forEach(s=> s.style.display = s.id === target ? '' : 'none');
    });
  });

  // sample data render for overview cards
  const stats = {
    orders: 128,
    sales: 542,
    revenue: '৳ 145,320',
    pendingPayouts: '৳ 12,400'
  };
  document.getElementById('statOrders').textContent = stats.orders;
  document.getElementById('statSales').textContent = stats.sales;
  document.getElementById('statRevenue').textContent = stats.revenue;
  document.getElementById('statPayouts').textContent = stats.pendingPayouts;

  // render a tiny SVG sparkline for analytics (simple random demo)
  (function drawSparkline(){
    const svg = document.getElementById('sparkline');
    if(!svg) return;
    const w = 300, h = 60; svg.setAttribute('viewBox', `0 0 ${w} ${h}`);
    const points = Array.from({length:18}, ()=> Math.random()*h*0.6 + h*0.2);
    const step = w / (points.length-1);
    const path = points.map((p,i)=> `${i===0?'M':'L'} ${i*step} ${h - p}`).join(' ');
    const ptEl = document.createElementNS('http://www.w3.org/2000/svg','path');
    ptEl.setAttribute('d', path); ptEl.setAttribute('fill','none'); ptEl.setAttribute('stroke','#7c3aed'); ptEl.setAttribute('stroke-width','2');
    svg.appendChild(ptEl);
  })();

  // dummy product list actions
  document.querySelectorAll('.product-delete').forEach(btn=> btn.addEventListener('click', (e)=>{
    const id = btn.dataset.id; if(!confirm('Delete product #' + id + '?')) return; btn.closest('tr').remove();
  }));

  // low-stock quick filter
  const lowBtn = document.getElementById('filterLowStock');
  if(lowBtn) lowBtn.addEventListener('click', ()=>{
    document.querySelectorAll('.orders-table tr[data-stock]').forEach(tr=>{
      const s = parseInt(tr.dataset.stock||'0',10); tr.style.display = s <= 5 ? '' : 'none';
    });
  });
});
