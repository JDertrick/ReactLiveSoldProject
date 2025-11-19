import React from "react";
import { CreateProductDto, UpdateProductDto } from "../../types/product.types";
import { useCategories } from "../../hooks/useCategories";
import { Label } from "../ui/label";
import { Input } from "../ui/input";
import { Textarea } from "../ui/textarea";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem
} from "../ui/select";

interface ProductFormProps {
  formData: CreateProductDto | UpdateProductDto;
  onFormChange: (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => void;
}

const ProductForm = ({ formData, onFormChange }: ProductFormProps) => {
  const { categories, isLoading: isLoadingCategories } = useCategories();

  const renderCategoryOptions = (categories: any[], depth = 0) => {
    return categories.map((category) => (
      <React.Fragment key={category.id}>
        <SelectItem value={category.id.toString()}>
          {"--".repeat(depth)} {category.name}
        </SelectItem>
        {category.children &&
          category.children.length > 0 &&
          renderCategoryOptions(category.children, depth + 1)}
      </React.Fragment>
    ));
  };

  return (
    <div className="space-y-4">
      <div>
        <Label className="text-xs font-semibold text-gray-500 tracking-wider">
          NOMBRE DEL PRODUCTO
        </Label>
        <div className="relative">
          <Input
            type="text"
            name="name"
            id="name"
            required
            value={formData.name}
            onChange={onFormChange}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
          />
        </div>
      </div>

      <div>
        <Label className="text-xs font-semibold text-gray-500 tracking-wider">
          DESCRIPCIÓN
        </Label>
        <Textarea
          name="description"
          id="description"
          rows={3}
          value={formData.description}
          onChange={onFormChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label className="text-xs font-semibold text-gray-500 tracking-wider">
            PRECIO BASE
          </Label>
          <div className="relative">
            <Input
              type="number"
              name="basePrice"
              id="basePrice"
              required
              step="0"
              placeholder="0"
              value={formData.basePrice}
              onChange={onFormChange}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
            />
          </div>
        </div>

        {/* Category Dropdown */}
        <div>
          <Label className="text-xs font-semibold text-gray-500 tracking-wider">
            CATEGORÍA
          </Label>
          <Select
            value={formData.categoryId?.toString() || ""}
            onValueChange={(value) => {
              const syntheticEvent = {
                target: {
                  name: "categoryId",
                  value: value,
                  type: "select"
                }
              } as unknown as React.ChangeEvent<HTMLSelectElement>;
              onFormChange(syntheticEvent);
            }}
            disabled={isLoadingCategories}
          >
            <SelectTrigger className="mt-1">
              <SelectValue placeholder="-- Seleccione una Categoría --" />
            </SelectTrigger>
            <SelectContent>
              {categories && renderCategoryOptions(categories)}
            </SelectContent>
          </Select>
        </div>
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
    </div>
  );
};

export default ProductForm;
