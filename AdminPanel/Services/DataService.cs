using AdminPanel.Models;
using System.Text.Json;

namespace AdminPanel.Services
{
    public class DataService
    {
        private readonly string _dataDir;
        private readonly string _categoriesFile;
        private readonly string _itemsFile;
        private static readonly JsonSerializerOptions JsonOptions = new() 
        { 
            WriteIndented = true,
            PropertyNameCaseInsensitive = true 
        };

        public DataService(string dataDir = "data")
        {
            _dataDir = dataDir;
            _categoriesFile = Path.Combine(_dataDir, "categories.json");
            _itemsFile = Path.Combine(_dataDir, "items.json");

            // Create data directory if not exists
            if (!Directory.Exists(_dataDir))
                Directory.CreateDirectory(_dataDir);

            // Initialize files if they don't exist
            if (!File.Exists(_categoriesFile))
                File.WriteAllText(_categoriesFile, JsonSerializer.Serialize(new List<Category>(), JsonOptions));

            if (!File.Exists(_itemsFile))
                File.WriteAllText(_itemsFile, JsonSerializer.Serialize(new List<Item>(), JsonOptions));
        }

        // Category Methods
        public List<Category> GetAllCategories()
        {
            var json = File.ReadAllText(_categoriesFile);
            return JsonSerializer.Deserialize<List<Category>>(json, JsonOptions) ?? new List<Category>();
        }

        public Category? GetCategoryById(int id)
        {
            var categories = GetAllCategories();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public void AddCategory(Category category)
        {
            var categories = GetAllCategories();
            category.Id = categories.Any() ? categories.Max(c => c.Id) + 1 : 1;
            categories.Add(category);
            SaveCategories(categories);
        }

        public void UpdateCategory(Category category)
        {
            var categories = GetAllCategories();
            var existing = categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.Icon = category.Icon;
                existing.IsActive = category.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                SaveCategories(categories);
            }
        }

        public void DeleteCategory(int id)
        {
            var categories = GetAllCategories();
            categories.RemoveAll(c => c.Id == id);
            SaveCategories(categories);

            // Also delete associated items
            DeleteItemsByCategory(id);
        }

        private void SaveCategories(List<Category> categories)
        {
            var json = JsonSerializer.Serialize(categories, JsonOptions);
            File.WriteAllText(_categoriesFile, json);
        }

        // Item Methods
        public List<Item> GetAllItems()
        {
            var json = File.ReadAllText(_itemsFile);
            return JsonSerializer.Deserialize<List<Item>>(json, JsonOptions) ?? new List<Item>();
        }

        public List<Item> GetItemsByCategory(int categoryId)
        {
            var items = GetAllItems();
            return items.Where(i => i.CategoryId == categoryId).ToList();
        }

        public Item? GetItemById(int id)
        {
            var items = GetAllItems();
            return items.FirstOrDefault(i => i.Id == id);
        }

        public void AddItem(Item item)
        {
            var items = GetAllItems();
            item.Id = items.Any() ? items.Max(i => i.Id) + 1 : 1;
            items.Add(item);
            SaveItems(items);
        }

        public void UpdateItem(Item item)
        {
            var items = GetAllItems();
            var existing = items.FirstOrDefault(i => i.Id == item.Id);
            if (existing != null)
            {
                existing.Name = item.Name;
                existing.Description = item.Description;
                existing.Price = item.Price;
                existing.ImageUrl = item.ImageUrl;
                existing.StockQuantity = item.StockQuantity;
                existing.IsActive = item.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                SaveItems(items);
            }
        }

        public void DeleteItem(int id)
        {
            var items = GetAllItems();
            items.RemoveAll(i => i.Id == id);
            SaveItems(items);
        }

        private void DeleteItemsByCategory(int categoryId)
        {
            var items = GetAllItems();
            items.RemoveAll(i => i.CategoryId == categoryId);
            SaveItems(items);
        }

        private void SaveItems(List<Item> items)
        {
            var json = JsonSerializer.Serialize(items, JsonOptions);
            File.WriteAllText(_itemsFile, json);
        }
    }
}
