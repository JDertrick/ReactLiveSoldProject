import { useState, useEffect } from "react";
import {
  CreateProductDto,
  UpdateProductDto,
  Product,
  Tag,
} from "../../types/product.types";
import ProductForm from "./ProductForm";
import { Box } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../ui/dialog";

interface ProductFormModalProps {
  isOpen: boolean;
  editingProduct: Product | null;
  isLoading: boolean;
  onClose: () => void;
  onSubmit: (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => Promise<void>;
  tags?: Tag[];
}

const ProductFormModal = ({
  isOpen,
  editingProduct,
  isLoading,
  onClose,
  onSubmit,
}: ProductFormModalProps) => {
  const [formData, setFormData] = useState<CreateProductDto | UpdateProductDto>(
    {
      name: "",
      description: "",
      basePrice: 0,
      isPublished: true,
      productType: "Variable",
      categoryId: "",
    }
  );

  // Actualizar formData cuando editingProduct cambie
  useEffect(() => {
    if (editingProduct) {
      setFormData({
        name: editingProduct.name,
        description: editingProduct.description || "",
        basePrice: editingProduct.basePrice,
        isPublished: editingProduct.isPublished,
        categoryId: editingProduct.categoryId || "",
        productType: editingProduct.productType,
      });
    } else {
      setFormData({
        name: "",
        description: "",
        basePrice: undefined,
        isPublished: true,
        productType: "Variable",
        categoryId: "",
      });
    }
  }, [editingProduct]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      // Guardar el producto
      await onSubmit(formData, !!editingProduct);
    } catch (error) {
      console.error("Error al guardar producto:", error);
      throw error;
    }
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value, type } = e.target;
    setFormData({
      ...formData,
      [name]:
        type === "checkbox"
          ? (e.target as HTMLInputElement).checked
          : type === "number"
          ? parseFloat(value) || 0
          : value,
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-xl max-h-[90vh] overflow-y-auto p-0">
        <form onSubmit={handleSubmit}>
          <DialogHeader className="p-6 pb-4">
            <div className="flex items-center gap-4">
              <div className="bg-indigo-100 p-3 rounded-lg">
                <Box className="w-6 h-6 text-indigo-600" />
              </div>
              <div>
                <DialogTitle className="text-xl font-bold">
                  Agregar producto
                </DialogTitle>
                <DialogDescription className="text-sm text-gray-500"></DialogDescription>
              </div>
            </div>
          </DialogHeader>
          <div className="p-6 space-y-6">
            <ProductForm formData={formData} onFormChange={handleChange} />
          </div>
          <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
            <button
              type="submit"
              disabled={isLoading}
              className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
            >
              {isLoading ? "Guardando..." : "Guardar Producto"}
            </button>
            <button
              type="button"
              onClick={onClose}
              className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
            >
              Cancelar
            </button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default ProductFormModal;
