import React from "react";
import { CreateProductDto, UpdateProductDto } from "../../types/product.types";
import { TagDto } from "../../types/product.types";
import { useCategories } from "../../hooks/useCategories";
import { ImageUpload } from "@/components/ui/image-upload";

interface ProductFormProps {
  formData: CreateProductDto | UpdateProductDto;
  tags: TagDto[] | undefined;
  onFormChange: (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => void;
  onTagToggle: (tagId: string) => void;
  onImageSelect?: (file: File) => void;
}

const ProductForm = ({
  formData,
  tags,
  onFormChange,
  onTagToggle,
  onImageSelect,
}: ProductFormProps) => {
  const { categories, isLoading: isLoadingCategories } = useCategories();
  console.log(formData);

  const renderCategoryOptions = (categories: any[], depth = 0) => {
    return categories.map((category) => (
      <React.Fragment key={category.id}>
        <option value={category.id}>
          {"--".repeat(depth)} {category.name}
        </option>
        {category.children &&
          category.children.length > 0 &&
          renderCategoryOptions(category.children, depth + 1)}
      </React.Fragment>
    ));
  };

  return (
    <div className="space-y-4">
      <div>
        <label
          htmlFor="name"
          className="block text-sm font-medium text-gray-700"
        >
          Nombre del Producto
        </label>
        <input
          type="text"
          name="name"
          id="name"
          required
          value={formData.name}
          onChange={onFormChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
        />
      </div>

      <div>
        <label
          htmlFor="description"
          className="block text-sm font-medium text-gray-700"
        >
          Descripción
        </label>
        <textarea
          name="description"
          id="description"
          rows={3}
          value={formData.description}
          onChange={onFormChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
        />
      </div>

      <div>
        <label
          htmlFor="basePrice"
          className="block text-sm font-medium text-gray-700"
        >
          Precio Base
        </label>
        <input
          type="number"
          name="basePrice"
          id="basePrice"
          required
          step="0.01"
          min="0"
          value={formData.basePrice}
          onChange={onFormChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
        />
      </div>

      <div>
        <ImageUpload
          label="Imagen del Producto"
          currentImageUrl={formData.imageUrl}
          onImageSelect={(file) => onImageSelect?.(file)}
          maxSizeMB={5}
        />
      </div>

      {/* Category Dropdown */}
      <div>
        <label
          htmlFor="categoryId"
          className="block text-sm font-medium text-gray-700"
        >
          Categoría
        </label>
        <select
          name="categoryId"
          id="categoryId"
          value={formData.categoryId || ""}
          onChange={onFormChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
          disabled={isLoadingCategories}
        >
          <option value="">-- Seleccione una Categoría --</option>
          {categories && renderCategoryOptions(categories)}
        </select>
      </div>

      <div className="flex items-center">
        <input
          id="isPublished"
          name="isPublished"
          type="checkbox"
          checked={formData.isPublished}
          onChange={onFormChange}
          className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
        />
        <label
          htmlFor="isPublished"
          className="ml-2 block text-sm text-gray-900"
        >
          Publicado (visible para los clientes)
        </label>
      </div>

      {/* Tags */}
      {tags && tags.length > 0 && (
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Etiquetas
          </label>
          <div className="flex flex-wrap gap-2">
            {tags.map((tag) => (
              <button
                key={tag.id}
                type="button"
                onClick={() => onTagToggle(tag.id)}
                className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                  (formData.tagIds || []).includes(tag.id)
                    ? "bg-indigo-600 text-white"
                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                }`}
              >
                {tag.name}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductForm;
