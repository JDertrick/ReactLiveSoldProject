import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useCategories } from "../../hooks/useCategories";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Textarea } from "../../components/ui/textarea";
import { toast } from "sonner";
import { Category } from "../../types/category.types";

const CategoryFormPage = () => {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();
  const { categories, createCategory, updateCategory, isLoading } = useCategories();

  const [formData, setFormData] = useState<Partial<Category>>({
    name: "",
    description: "",
    parentId: undefined,
  });

  useEffect(() => {
    if (id && categories) {
      const categoryToEdit = categories.find((cat) => cat.id === id);
      if (categoryToEdit) {
        setFormData({
          name: categoryToEdit.name,
          description: categoryToEdit.description,
          parentId: categoryToEdit.parentId,
        });
      }
    }
  }, [id, categories]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value === "" ? undefined : value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateCategory({ ...formData, id } as Category);
        toast.success("Category updated successfully!");
      } else {
        await createCategory(formData as Category);
        toast.success("Category created successfully!");
      }
      navigate("/app/categories");
    } catch (err) {
      toast.error("Failed to save category.");
      console.error("Error saving category:", err);
    }
  };

  const renderCategoryOptions = (cats: Category[], depth = 0) => {
    return cats.map((category) => (
      <option key={category.id} value={category.id} disabled={category.id === id}>
        {"--".repeat(depth)} {category.name}
      </option>
    ));
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-6">
        {id ? "Edit Category" : "Create New Category"}
      </h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700">
            Name
          </label>
          <Input
            type="text"
            name="name"
            id="name"
            required
            value={formData.name || ""}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="description" className="block text-sm font-medium text-gray-700">
            Description
          </label>
          <Textarea
            name="description"
            id="description"
            value={formData.description || ""}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="parentId" className="block text-sm font-medium text-gray-700">
            Parent Category
          </label>
          <select
            name="parentId"
            id="parentId"
            value={formData.parentId || ""}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
          >
            <option value="">-- No Parent --</option>
            {categories && renderCategoryOptions(categories)}
          </select>
        </div>
        <div className="flex gap-2">
          <Button type="submit">{id ? "Update Category" : "Create Category"}</Button>
          <Button type="button" variant="outline" onClick={() => navigate("/app/categories")}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
};

export default CategoryFormPage;
