import { useState, useEffect } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import {
  CreateProductDto,
  UpdateProductDto,
  Product,
  ProductVariantDto,
  TagDto,
} from "../../types/product.types";
import ProductForm from "./ProductForm";
import { useUploadProductImage } from "../../hooks/useProducts";

interface ProductFormModalProps {
  isOpen: boolean;
  editingProduct: Product | null;
  tags: TagDto[] | undefined;
  isLoading: boolean;
  variants: ProductVariantDto[];
  onClose: () => void;
  onSubmit: (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => Promise<void>;
  onOpenVariantModal: () => void;
}

const ProductFormModal = ({
  isOpen,
  editingProduct,
  tags,
  isLoading,
  variants,
  onClose,
  onSubmit,
  onOpenVariantModal,
}: ProductFormModalProps) => {
  const uploadProductImage = useUploadProductImage();
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [formData, setFormData] = useState<CreateProductDto | UpdateProductDto>(
    {
      name: "",
      description: "",
      basePrice: 0,
      imageUrl: "",
      isPublished: true,
      productType: "",
      categoryId: "",
      variants: [],
      tagIds: [],
    }
  );

  // Actualizar formData cuando editingProduct cambie
  useEffect(() => {
    if (editingProduct) {
      setFormData({
        name: editingProduct.name,
        description: editingProduct.description || "",
        basePrice: editingProduct.basePrice,
        imageUrl: editingProduct.imageUrl || "",
        isPublished: editingProduct.isPublished,
        categoryId: editingProduct.categoryId || "",
        // No necesitamos las variantes aquí, se manejan por separado
        variants: [],
        tagIds: editingProduct.tags?.map((t) => t.id) || [],
        productType: editingProduct.productType,
      });
    } else {
      setFormData({
        name: "",
        description: "",
        basePrice: 0,
        imageUrl: "",
        isPublished: true,
        productType: "",
        categoryId: "",
        variants: [],
        tagIds: [],
      });
    }
  }, [editingProduct]);

  // Actualizar variantes cuando cambien desde el modal de variantes
  useEffect(() => {
    setFormData((prev) => ({
      ...prev,
      variants: variants.map((v) => ({
        sku: v.sku,
        price: v.price,
        stockQuantity: v.stockQuantity || v.stock,
        attributes: v.attributes,
        imageUrl: v.imageUrl,
      })),
    }));
  }, [variants]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      // Si no estamos editando, establecer el productType
      if (!editingProduct) {
        formData.productType = "Variable";
      }

      // Guardar el producto primero
      await onSubmit(formData, !!editingProduct);

      // Si hay una imagen seleccionada y estamos editando un producto, subirla
      if (selectedImage && editingProduct) {
        const result = await uploadProductImage.mutateAsync({
          productId: editingProduct.id,
          image: selectedImage,
        });

        // Actualizar el formData con la nueva URL de imagen
        setFormData(prev => ({
          ...prev,
          imageUrl: result.imageUrl || ""
        }));
      }

      // Limpiar la imagen seleccionada
      setSelectedImage(null);
    } catch (error) {
      console.error("Error al guardar producto:", error);
      throw error;
    }
  };

  const handleImageSelect = (file: File) => {
    setSelectedImage(file);
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

  const handleTagToggle = (tagId: string) => {
    const currentTags = formData.tagIds || [];
    if (currentTags.includes(tagId)) {
      setFormData({
        ...formData,
        tagIds: currentTags.filter((id) => id !== tagId),
      });
    } else {
      setFormData({
        ...formData,
        tagIds: [...currentTags, tagId],
      });
    }
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-10">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
      />

      <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
        <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
          <DialogPanel
            transition
            className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-xl data-closed:sm:translate-y-0 data-closed:sm:scale-95"
          >
            <form onSubmit={handleSubmit}>
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                <DialogTitle
                  as="h3"
                  className="text-lg leading-6 font-medium text-gray-900 mb-4"
                >
                  {editingProduct ? "Editar Producto" : "Crear Producto"}
                </DialogTitle>

                <div className="space-y-6">
                  {/* Product Form */}
                  <ProductForm
                    formData={formData}
                    tags={tags}
                    onFormChange={handleChange}
                    onTagToggle={handleTagToggle}
                    onImageSelect={handleImageSelect}
                  />

                  {/* Variants Section */}
                  <div className="border-t pt-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="text-sm font-medium text-gray-700">
                          Variantes de Producto
                        </h4>
                        <p className="text-sm text-gray-500 mt-1">
                          {variants.length > 0
                            ? `${variants.length} variante${
                                variants.length !== 1 ? "s" : ""
                              } configurada${variants.length !== 1 ? "s" : ""}`
                            : "Aún no hay variantes configuradas"}
                        </p>
                      </div>
                      <button
                        type="button"
                        onClick={onOpenVariantModal}
                        className="inline-flex items-center px-4 py-2 border border-indigo-600 rounded-md shadow-sm text-sm font-medium text-indigo-600 bg-white hover:bg-indigo-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                      >
                        <svg
                          className="h-5 w-5 mr-2"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                          />
                        </svg>
                        Gestionar Variantes
                      </button>
                    </div>

                    {/* Display variants preview */}
                    {variants.length > 0 && (
                      <div className="mt-4 grid grid-cols-2 gap-2">
                        {variants.slice(0, 4).map((variant, index) => (
                          <div
                            key={index}
                            className="text-xs bg-gray-50 rounded p-2 border border-gray-200"
                          >
                            <p className="font-medium text-gray-900">
                              {variant.sku}
                            </p>
                            <p className="text-gray-500">
                              {variant.size && `Talla: ${variant.size}`}
                              {variant.size && variant.color && " • "}
                              {variant.color && `Color: ${variant.color}`}
                            </p>
                            <p className="text-gray-600 mt-1">
                              Stock: {variant.stock}
                            </p>
                          </div>
                        ))}
                        {variants.length > 4 && (
                          <div className="text-xs bg-gray-50 rounded p-2 border border-gray-200 flex items-center justify-center text-gray-500">
                            +{variants.length - 4} más
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
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
          </DialogPanel>
        </div>
      </div>
    </Dialog>
  );
};

export default ProductFormModal;
