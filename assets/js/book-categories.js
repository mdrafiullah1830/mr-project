/* === Book Categories JavaScript === */

// Categories Data
const categoriesData = [
  {
    id: 1,
    name: 'Literature',
    icon: '📚',
    subcategories: [
      { name: 'Classic Literature', count: 245, icon: '📖' },
      { name: 'Contemporary Fiction', count: 312, icon: '✨' },
      { name: 'Poetry', count: 156, icon: '🎭' },
      { name: 'Drama & Plays', count: 98, icon: '🎪' },
      { name: 'Short Stories', count: 187, icon: '📝' },
      { name: 'Essays & Articles', count: 134, icon: '📄' },
      { name: 'Literary Criticism', count: 76, icon: '🔍' },
      { name: 'World Literature', count: 289, icon: '🌍' }
    ]
  },
  {
    id: 2,
    name: 'Novel',
    icon: '📕',
    subcategories: [
      { name: 'Romance Novel', count: 428, icon: '💕' },
      { name: 'Mystery & Thriller', count: 356, icon: '🔎' },
      { name: 'Historical Novel', count: 198, icon: '⏳' },
      { name: 'Adventure Novel', count: 267, icon: '🗺️' },
      { name: 'Psychological Fiction', count: 145, icon: '🧠' },
      { name: 'Literary Fiction', count: 223, icon: '✍️' }
    ]
  },
  {
    id: 3,
    name: 'Science Fiction',
    icon: '🚀',
    subcategories: [
      { name: 'Space Opera', count: 189, icon: '🌌' },
      { name: 'Cyberpunk', count: 143, icon: '🤖' },
      { name: 'Time Travel', count: 112, icon: '⏰' },
      { name: 'Dystopian Sci-Fi', count: 167, icon: '🏙️' },
      { name: 'Hard Science Fiction', count: 98, icon: '🔬' },
      { name: 'Alien Contact', count: 134, icon: '👽' },
      { name: 'Post-Apocalyptic', count: 156, icon: '🌪️' }
    ]
  },
  {
    id: 4,
    name: 'Islamic Books',
    icon: '☪️',
    subcategories: [
      { name: 'Quran & Tafsir', count: 287, icon: '📗' },
      { name: 'Hadith Collections', count: 198, icon: '📜' },
      { name: 'Islamic History', count: 245, icon: '🕌' },
      { name: 'Fiqh & Jurisprudence', count: 156, icon: '⚖️' },
      { name: 'Seerah (Biography)', count: 167, icon: '👤' },
      { name: 'Islamic Philosophy', count: 123, icon: '💭' },
      { name: 'Dua & Supplications', count: 89, icon: '🤲' },
      { name: 'Islamic Children Books', count: 134, icon: '👶' }
    ]
  },
  {
    id: 5,
    name: 'Academic Books',
    icon: '🎓',
    subcategories: [
      { name: 'Mathematics', count: 312, icon: '➗' },
      { name: 'Physics', count: 267, icon: '⚛️' },
      { name: 'Chemistry', count: 234, icon: '🧪' },
      { name: 'Biology', count: 289, icon: '🧬' },
      { name: 'Computer Science', count: 398, icon: '💻' },
      { name: 'Engineering', count: 345, icon: '⚙️' },
      { name: 'Medicine', count: 423, icon: '🏥' },
      { name: 'Economics', count: 198, icon: '📊' },
      { name: 'Business Studies', count: 256, icon: '💼' }
    ]
  },
  {
    id: 6,
    name: 'Children Books',
    icon: '🧸',
    subcategories: [
      { name: 'Picture Books', count: 412, icon: '🎨' },
      { name: 'Fairy Tales', count: 267, icon: '🧚' },
      { name: 'Adventure Stories', count: 198, icon: '🏴‍☠️' },
      { name: 'Educational Books', count: 345, icon: '📚' },
      { name: 'Activity Books', count: 223, icon: '✂️' },
      { name: 'Bedtime Stories', count: 289, icon: '🌙' }
    ]
  },
  {
    id: 7,
    name: 'Biography',
    icon: '👤',
    subcategories: [
      { name: 'Autobiography', count: 178, icon: '✍️' },
      { name: 'Historical Figures', count: 234, icon: '🏛️' },
      { name: 'Political Leaders', count: 156, icon: '🎖️' },
      { name: 'Artists & Writers', count: 189, icon: '🎨' },
      { name: 'Scientists', count: 145, icon: '🔬' },
      { name: 'Sports Personalities', count: 123, icon: '🏆' }
    ]
  },
  {
    id: 8,
    name: 'Thriller',
    icon: '🔪',
    subcategories: [
      { name: 'Crime Thriller', count: 298, icon: '🚨' },
      { name: 'Psychological Thriller', count: 267, icon: '🧠' },
      { name: 'Spy Thriller', count: 189, icon: '🕵️' },
      { name: 'Legal Thriller', count: 134, icon: '⚖️' },
      { name: 'Medical Thriller', count: 112, icon: '💉' },
      { name: 'Techno-Thriller', count: 145, icon: '💻' }
    ]
  },
  {
    id: 9,
    name: 'History',
    icon: '📜',
    subcategories: [
      { name: 'World History', count: 389, icon: '🌍' },
      { name: 'Ancient History', count: 267, icon: '🏛️' },
      { name: 'Medieval History', count: 198, icon: '⚔️' },
      { name: 'Modern History', count: 312, icon: '🏭' },
      { name: 'Military History', count: 234, icon: '🎖️' },
      { name: 'Cultural History', count: 178, icon: '🎭' }
    ]
  },
  {
    id: 10,
    name: 'Self-Help',
    icon: '💪',
    subcategories: [
      { name: 'Motivation', count: 345, icon: '🔥' },
      { name: 'Personal Development', count: 412, icon: '📈' },
      { name: 'Productivity', count: 289, icon: '⏱️' },
      { name: 'Mindfulness', count: 234, icon: '🧘' },
      { name: 'Leadership', count: 267, icon: '👔' },
      { name: 'Communication Skills', count: 198, icon: '🗣️' }
    ]
  },
  {
    id: 11,
    name: 'Fantasy',
    icon: '🐉',
    subcategories: [
      { name: 'Epic Fantasy', count: 312, icon: '⚔️' },
      { name: 'Urban Fantasy', count: 267, icon: '🏙️' },
      { name: 'Dark Fantasy', count: 189, icon: '🌑' },
      { name: 'Magical Realism', count: 156, icon: '✨' },
      { name: 'Sword & Sorcery', count: 198, icon: '🗡️' },
      { name: 'High Fantasy', count: 223, icon: '🏰' }
    ]
  },
  {
    id: 12,
    name: 'Comics & Manga',
    icon: '📱',
    subcategories: [
      { name: 'Superhero Comics', count: 398, icon: '🦸' },
      { name: 'Manga Series', count: 512, icon: '🎌' },
      { name: 'Graphic Novels', count: 289, icon: '🎨' },
      { name: 'Webcomics', count: 234, icon: '💻' },
      { name: 'Comic Strips', count: 167, icon: '📰' }
    ]
  }
];

// State
let currentCategory = categoriesData[0];
let filteredCategories = [...categoriesData];

// DOM Elements
const categoryList = document.getElementById('categoryList');
const categoryCount = document.getElementById('categoryCount');
const contentTitle = document.getElementById('contentTitle');
const subcategoryCount = document.getElementById('subcategoryCount');
const subcategoryGrid = document.getElementById('subcategoryGrid');
const emptyState = document.getElementById('emptyState');
const searchInput = document.getElementById('categorySearch');
const themeToggle = document.getElementById('themeToggle');

// === Theme Toggle ===
const savedTheme = localStorage.getItem('mrshop-theme') || 'light';
if (savedTheme === 'dark') {
  document.documentElement.classList.add('theme-dark');
  themeToggle.textContent = '☀️';
}

themeToggle.addEventListener('click', () => {
  document.documentElement.classList.toggle('theme-dark');
  const isDark = document.documentElement.classList.contains('theme-dark');
  themeToggle.textContent = isDark ? '☀️' : '🌙';
  localStorage.setItem('mrshop-theme', isDark ? 'dark' : 'light');
});

// === Render Categories ===
function renderCategories(categories) {
  categoryList.innerHTML = categories.map((cat, index) => `
    <li class="bc-category-item ${index === 0 ? 'active' : ''}" data-id="${cat.id}">
      <span class="bc-category-icon">${cat.icon}</span>
      <span class="bc-category-text">${cat.name}</span>
      <span class="bc-category-arrow">→</span>
    </li>
  `).join('');

  categoryCount.textContent = categories.length;

  // Add click listeners
  document.querySelectorAll('.bc-category-item').forEach((item) => {
    item.addEventListener('click', () => {
      const categoryId = parseInt(item.dataset.id);
      selectCategory(categoryId);
    });
  });
}

// === Select Category ===
function selectCategory(categoryId) {
  // Update active state
  document.querySelectorAll('.bc-category-item').forEach((item) => {
    item.classList.remove('active');
  });
  document.querySelector(`[data-id="${categoryId}"]`).classList.add('active');

  // Find category
  const category = categoriesData.find((cat) => cat.id === categoryId);
  if (!category) return;

  currentCategory = category;
  renderSubcategories(category);
}

// === Render Subcategories ===
function renderSubcategories(category) {
  contentTitle.textContent = category.name;
  subcategoryCount.textContent = `${category.subcategories.length} subcategories`;

  if (category.subcategories.length === 0) {
    subcategoryGrid.style.display = 'none';
    emptyState.style.display = 'flex';
    return;
  }

  subcategoryGrid.style.display = 'grid';
  emptyState.style.display = 'none';

  subcategoryGrid.innerHTML = category.subcategories.map((sub) => `
    <a href="#" class="bc-subcategory-card">
      <div class="bc-subcategory-header">
        <div class="bc-subcategory-icon">${sub.icon}</div>
        <span class="bc-subcategory-badge">${sub.count}</span>
      </div>
      <div class="bc-subcategory-name">${sub.name}</div>
      <div class="bc-subcategory-count-text">${sub.count} books available</div>
    </a>
  `).join('');
}

// === Search Categories ===
searchInput.addEventListener('input', (e) => {
  const query = e.target.value.toLowerCase().trim();

  if (!query) {
    filteredCategories = [...categoriesData];
    renderCategories(filteredCategories);
    selectCategory(categoriesData[0].id);
    return;
  }

  filteredCategories = categoriesData.filter((cat) => {
    const nameMatch = cat.name.toLowerCase().includes(query);
    const subMatch = cat.subcategories.some((sub) =>
      sub.name.toLowerCase().includes(query)
    );
    return nameMatch || subMatch;
  });

  if (filteredCategories.length === 0) {
    categoryList.innerHTML = '<p style="padding: 20px; text-align: center; color: var(--bc-text-light);">No categories found</p>';
    categoryCount.textContent = '0';
    subcategoryGrid.style.display = 'none';
    emptyState.style.display = 'flex';
  } else {
    renderCategories(filteredCategories);
    selectCategory(filteredCategories[0].id);
  }
});

// === Initialize ===
renderCategories(filteredCategories);
renderSubcategories(currentCategory);
