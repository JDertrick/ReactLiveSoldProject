import { useState } from "react";
import {
  useGetProducts,
  useCreateProduct,
  useUpdateProduct,
  useGetTags,
} from "../../hooks/useProducts";
import {
  CreateProductDto,
  UpdateProductDto,
  Product,
  ProductVariantDto,
  CreateProductVariantDto,
  ProductVariant,
} from "../../types/product.types";
import ProductFormModal from "../../components/products/ProductFormModal";
import VariantModal from "../../components/products/VariantModal";

const ProductsPage = () => {
  const { data: products, isLoading } = useGetProducts(true); // Include unpublished
  const { data: tags } = useGetTags();
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isVariantModalOpen, setIsVariantModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [variants, setVariants] = useState<ProductVariantDto[]>([]);

  // Helper: Convertir ProductVariant (backend) a ProductVariantDto (formulario)
  const convertToVariantDto = (variant: ProductVariant): ProductVariantDto => ({
    id: variant.id,
    sku: variant.sku || "",
    size: variant.size || undefined,
    color: variant.color || undefined,
    price: variant.price,
    stock: variant.stockQuantity,
    imageUrl: variant.imageUrl,
  });

  // Helper: Convertir ProductVariantDto (formulario) a CreateProductVariantDto (backend)
  const convertToCreateVariantDto = (variant: ProductVariantDto): CreateProductVariantDto => {
    const attributes: Record<string, string> = {};
    if (variant.size) attributes.size = variant.size;
    if (variant.color) attributes.color = variant.color;

    return {
      sku: variant.sku,
      price: variant.price,
      stockQuantity: variant.stock,
      attributes: Object.keys(attributes).length > 0 ? JSON.stringify(attributes) : undefined,
      imageUrl: variant.imageUrl,
    };
  };

  const handleOpenModal = (product?: Product) => {
    setEditingProduct(product || null);
    const convertedVariants = product?.variants?.map(convertToVariantDto) || [];
    setVariants(convertedVariants);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingProduct(null);
    setVariants([]);
  };

  const handleOpenVariantModal = () => {
    setIsVariantModalOpen(true);
  };

  const handleCloseVariantModal = () => {
    setIsVariantModalOpen(false);
  };

  const handleVariantsChange = (newVariants: ProductVariantDto[]) => {
    setVariants(newVariants);
  };

  const handleSubmit = async (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => {
    try {
      if (isEditing && editingProduct) {
        // Actualizar producto con variantes
        const updateData: UpdateProductDto = {
          ...(data as UpdateProductDto),
          variants: variants.map(convertToCreateVariantDto),
        };

        await updateProduct.mutateAsync({
          id: editingProduct.id,
          data: updateData,
        });
      } else {
        // Crear producto nuevo con variantes
        const createData: CreateProductDto = {
          ...(data as CreateProductDto),
          variants: variants.map(convertToCreateVariantDto),
        };
        await createProduct.mutateAsync(createData);
      }
      handleCloseModal();
    } catch (error) {
      console.error("Error saving product:", error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-semibold text-gray-900">Products</h1>
          <p className="mt-2 text-sm text-gray-700">
            Manage your product catalog, variants, and inventory levels.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <button
            type="button"
            onClick={() => handleOpenModal()}
            className="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 sm:w-auto"
          >
            Add Product
          </button>
        </div>
      </div>

      <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {products && products.length > 0 ? (
          products.map((product) => {
            const totalStock =
              product.variants?.reduce((sum, v) => sum + v.stock, 0) || 0;
            return (
              <div
                key={product.id}
                className="bg-white overflow-hidden shadow rounded-lg"
              >
                {product.imageUrl && (
                  <div className="h-48 w-full overflow-hidden bg-gray-200">
                    <img
                      src={product.imageUrl}
                      alt={product.name}
                      className="h-full w-full object-cover"
                    />
                  </div>
                )}
                <div className="px-4 py-5 sm:p-6">
                  <h3 className="text-lg font-medium text-gray-900 truncate">
                    {product.name}
                  </h3>
                  {product.description && (
                    <p className="mt-1 text-sm text-gray-500 line-clamp-2">
                      {product.description}
                    </p>
                  )}
                  <div className="mt-3 flex items-center justify-between">
                    <span className="text-xl font-bold text-gray-900">
                      ${(product.basePrice || 0).toFixed(2)}
                    </span>
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        product.isPublished
                          ? "bg-green-100 text-green-800"
                          : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {product.isPublished ? "Published" : "Draft"}
                    </span>
                  </div>
                  <div className="mt-2 flex items-center text-sm text-gray-500">
                    <svg
                      className="flex-shrink-0 mr-1.5 h-5 w-5 text-gray-400"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
                      />
                    </svg>
                    {totalStock} units in stock •{" "}
                    {product.variants?.length || 0} variants
                  </div>
                  {product.tags && product.tags.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-1">
                      {product.tags.map((tag) => (
                        <span
                          key={tag.id}
                          className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-indigo-100 text-indigo-800"
                        >
                          {tag.name}
                        </span>
                      ))}
                    </div>
                  )}
                  <div className="mt-4">
                    <button
                      onClick={() => handleOpenModal(product)}
                      className="w-full inline-flex justify-center items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Edit Product
                    </button>
                  </div>
                </div>
              </div>
            );
          })
        ) : (
          <div className="col-span-full text-center py-12">
            <p className="text-sm text-gray-500">
              No products found. Add your first product to get started.
            </p>
          </div>
        )}
      </div>

      {/* Create/Edit Product Modal */}
      <ProductFormModal
        isOpen={isModalOpen}
        editingProduct={editingProduct}
        tags={tags}
        variants={variants}
        isLoading={createProduct.isPending || updateProduct.isPending}
        onClose={handleCloseModal}
        onSubmit={handleSubmit}
        onOpenVariantModal={handleOpenVariantModal}
      />

      {/* Variant Management Modal */}
      <VariantModal
        isOpen={isVariantModalOpen}
        productName={editingProduct?.name || "New Product"}
        variants={variants}
        onClose={handleCloseVariantModal}
        onSaveVariants={handleVariantsChange}
      />
    </div>
  );
};

export default ProductsPage;
