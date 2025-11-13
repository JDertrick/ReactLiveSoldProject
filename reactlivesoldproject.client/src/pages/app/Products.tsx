import { useState } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
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
} from "../../types/product.types";

const ProductsPage = () => {
  const { data: products, isLoading } = useGetProducts(true); // Include unpublished
  const { data: tags } = useGetTags();
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [formData, setFormData] = useState<CreateProductDto | UpdateProductDto>(
    {
      name: "",
      description: "",
      basePrice: 0,
      imageUrl: "",
      isPublished: true,
      variants: [],
      tagIds: [],
      productType: "Variable",
    }
  );

  const [variantInput, setVariantInput] = useState({
    sku: "",
    size: "",
    color: "",
    stock: "",
    price: "",
  });

  const [editingVariantIndex, setEditingVariantIndex] = useState<number | null>(
    null
  );

  const handleOpenModal = (product?: Product) => {
    if (product) {
      setEditingProduct(product);
      setFormData({
        name: product.name,
        description: product.description || "",
        basePrice: product.basePrice,
        imageUrl: product.imageUrl || "",
        isPublished: product.isPublished,
        variants: product.variants || [],
        tagIds: product.tags?.map((t) => t.id) || [],
        productType: product.productType,
      });
    } else {
      setEditingProduct(null);
      setFormData({
        name: "",
        description: "",
        basePrice: 0,
        imageUrl: "",
        isPublished: true,
        productType: "",
        variants: [],
        tagIds: [],
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingProduct(null);
    setVariantInput({ sku: "", size: "", color: "", stock: "", price: "" });
    setEditingVariantIndex(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingProduct) {
        await updateProduct.mutateAsync({
          id: editingProduct.id,
          data: formData as UpdateProductDto,
        });
      } else {
        formData.productType = "Variable";
        await createProduct.mutateAsync(formData as CreateProductDto);
      }
      handleCloseModal();
    } catch (error) {
      console.error("Error saving product:", error);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
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

  const handleAddVariant = () => {
    const stock = parseInt(variantInput.stock) || 0;
    const price = parseFloat(variantInput.price) || 0;

    if (variantInput.sku && stock >= 0) {
      const newVariant: ProductVariantDto = {
        sku: variantInput.sku,
        size: variantInput.size || undefined,
        color: variantInput.color || undefined,
        stock,
        price,
      };

      if (editingVariantIndex !== null) {
        // Estamos editando una variante existente
        const newVariants = [...(formData.variants || [])];
        newVariants[editingVariantIndex] = newVariant;
        setFormData({ ...formData, variants: newVariants });
        setEditingVariantIndex(null);
      } else {
        // Agregar nueva variante
        setFormData({
          ...formData,
          variants: [...(formData.variants || []), newVariant],
        });
      }

      setVariantInput({ sku: "", size: "", color: "", stock: "", price: "" });
    }
  };

  const handleEditVariant = (index: number) => {
    const variant = formData.variants?.[index];
    if (variant) {
      setVariantInput({
        sku: variant.sku,
        size: variant.size || "",
        color: variant.color || "",
        stock: variant.stock.toString(),
        price: variant.price.toString(),
      });
      setEditingVariantIndex(index);
    }
  };

  const handleCancelEditVariant = () => {
    setVariantInput({ sku: "", size: "", color: "", stock: "", price: "" });
    setEditingVariantIndex(null);
  };

  const handleRemoveVariant = (index: number) => {
    const newVariants = [...(formData.variants || [])];
    newVariants.splice(index, 1);
    setFormData({ ...formData, variants: newVariants });

    // Si estábamos editando esta variante, cancelar la edición
    if (editingVariantIndex === index) {
      handleCancelEditVariant();
    } else if (editingVariantIndex !== null && editingVariantIndex > index) {
      // Ajustar el índice si eliminamos una variante antes de la que estamos editando
      setEditingVariantIndex(editingVariantIndex - 1);
    }
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

      {/* Create/Edit Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleCloseModal}
        className="relative z-10"
      >
        <DialogBackdrop
          transition
          className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
        />

        <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <DialogPanel
              transition
              className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-4xl data-closed:sm:translate-y-0 data-closed:sm:scale-95"
            >
              <form onSubmit={handleSubmit}>
                <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                  <DialogTitle
                    as="h3"
                    className="text-lg leading-6 font-medium text-gray-900 mb-4"
                  >
                    {editingProduct ? "Edit Product" : "Create Product"}
                  </DialogTitle>

                  <div className="grid grid-cols-2 gap-6">
                    {/* Left Column */}
                    <div className="space-y-4">
                      <div>
                        <label
                          htmlFor="name"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Product Name
                        </label>
                        <input
                          type="text"
                          name="name"
                          id="name"
                          required
                          value={formData.name}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="description"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Description
                        </label>
                        <textarea
                          name="description"
                          id="description"
                          rows={3}
                          value={formData.description}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="basePrice"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Base Price
                        </label>
                        <input
                          type="number"
                          name="basePrice"
                          id="basePrice"
                          required
                          step="0.01"
                          min="0"
                          value={formData.basePrice}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="imageUrl"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Image URL
                        </label>
                        <input
                          type="url"
                          name="imageUrl"
                          id="imageUrl"
                          value={formData.imageUrl}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>

                      <div className="flex items-center">
                        <input
                          id="isPublished"
                          name="isPublished"
                          type="checkbox"
                          checked={formData.isPublished}
                          onChange={handleChange}
                          className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                        />
                        <label
                          htmlFor="isPublished"
                          className="ml-2 block text-sm text-gray-900"
                        >
                          Published (visible to customers)
                        </label>
                      </div>

                      {/* Tags */}
                      {tags && tags.length > 0 && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Tags
                          </label>
                          <div className="flex flex-wrap gap-2">
                            {tags.map((tag) => (
                              <button
                                key={tag.id}
                                type="button"
                                onClick={() => handleTagToggle(tag.id)}
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

                    {/* Right Column - Variants */}
                    <div>
                      <h4 className="text-sm font-medium text-gray-700 mb-3">
                        Product Variants
                      </h4>

                      {/* Variant Input */}
                      <div className="bg-gray-50 p-4 rounded-lg mb-4">
                        {editingVariantIndex !== null && (
                          <div className="mb-3 flex items-center justify-between bg-blue-50 border border-blue-200 rounded px-3 py-2">
                            <span className="text-sm text-blue-700 font-medium">
                              Editing variant #{editingVariantIndex + 1}
                            </span>
                            <button
                              type="button"
                              onClick={handleCancelEditVariant}
                              className="text-sm text-blue-600 hover:text-blue-800"
                            >
                              Cancel
                            </button>
                          </div>
                        )}
                        <div className="grid grid-cols-2 gap-3 mb-3">
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              SKU *
                            </label>
                            <input
                              type="text"
                              placeholder="ABC-123"
                              value={variantInput.sku}
                              onChange={(e) =>
                                setVariantInput({
                                  ...variantInput,
                                  sku: e.target.value,
                                })
                              }
                              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Talla
                            </label>
                            <input
                              type="text"
                              placeholder="M, L, XL..."
                              value={variantInput.size}
                              onChange={(e) =>
                                setVariantInput({
                                  ...variantInput,
                                  size: e.target.value,
                                })
                              }
                              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Color
                            </label>
                            <input
                              type="text"
                              placeholder="Rojo, Azul..."
                              value={variantInput.color}
                              onChange={(e) =>
                                setVariantInput({
                                  ...variantInput,
                                  color: e.target.value,
                                })
                              }
                              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Cantidad *
                            </label>
                            <input
                              type="number"
                              placeholder="0"
                              min="0"
                              value={variantInput.stock}
                              onChange={(e) =>
                                setVariantInput({
                                  ...variantInput,
                                  stock: e.target.value,
                                })
                              }
                              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            />
                          </div>
                          <div className="col-span-2">
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Precio (opcional, usa precio base si está vacío)
                            </label>
                            <input
                              type="number"
                              placeholder="0.00"
                              step="0.01"
                              min="0"
                              value={variantInput.price}
                              onChange={(e) =>
                                setVariantInput({
                                  ...variantInput,
                                  price: e.target.value,
                                })
                              }
                              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            />
                          </div>
                        </div>
                        <button
                          type="button"
                          onClick={handleAddVariant}
                          className="w-full inline-flex justify-center items-center px-3 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700"
                        >
                          {editingVariantIndex !== null
                            ? "Update Variant"
                            : "Add Variant"}
                        </button>
                      </div>

                      {/* Variants List */}
                      <div className="space-y-2 max-h-96 overflow-y-auto">
                        {formData.variants && formData.variants.length > 0 ? (
                          formData.variants.map((variant, index) => (
                            <div
                              key={index}
                              className={`flex items-center justify-between p-3 bg-white border rounded-lg ${
                                editingVariantIndex === index
                                  ? "border-blue-400 bg-blue-50"
                                  : "border-gray-200"
                              }`}
                            >
                              <div className="flex-1">
                                <p className="text-sm font-medium text-gray-900">
                                  {variant.sku}
                                </p>
                                <p className="text-xs text-gray-500">
                                  {variant.size && `Talla: ${variant.size}`}
                                  {variant.size && variant.color && " • "}
                                  {variant.color && `Color: ${variant.color}`}
                                </p>
                                <p className="text-xs text-gray-500 mt-1">
                                  <span className="font-semibold">
                                    Cantidad: {variant.stock}
                                  </span>
                                  {variant.price > 0 &&
                                    ` • Precio: $${(variant.price || 0).toFixed(
                                      2
                                    )}`}
                                </p>
                              </div>
                              <div className="flex items-center gap-2 ml-2">
                                <button
                                  type="button"
                                  onClick={() => handleEditVariant(index)}
                                  className="text-indigo-600 hover:text-indigo-800"
                                  title="Edit variant"
                                >
                                  <svg
                                    className="h-5 w-5"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                  >
                                    <path
                                      strokeLinecap="round"
                                      strokeLinejoin="round"
                                      strokeWidth={2}
                                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                                    />
                                  </svg>
                                </button>
                                <button
                                  type="button"
                                  onClick={() => handleRemoveVariant(index)}
                                  className="text-red-600 hover:text-red-800"
                                  title="Delete variant"
                                >
                                  <svg
                                    className="h-5 w-5"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                  >
                                    <path
                                      strokeLinecap="round"
                                      strokeLinejoin="round"
                                      strokeWidth={2}
                                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                                    />
                                  </svg>
                                </button>
                              </div>
                            </div>
                          ))
                        ) : (
                          <p className="text-sm text-gray-500 text-center py-4">
                            No variants added yet
                          </p>
                        )}
                      </div>
                    </div>
                  </div>
                </div>

                <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                  <button
                    type="submit"
                    disabled={
                      createProduct.isPending || updateProduct.isPending
                    }
                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
                  >
                    {createProduct.isPending || updateProduct.isPending
                      ? "Saving..."
                      : "Save Product"}
                  </button>
                  <button
                    type="button"
                    onClick={handleCloseModal}
                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </DialogPanel>
          </div>
        </div>
      </Dialog>
    </div>
  );
};

export default ProductsPage;
