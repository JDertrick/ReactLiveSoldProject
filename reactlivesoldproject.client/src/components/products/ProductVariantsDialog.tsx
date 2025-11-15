import { useState, useEffect } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import {
  Product,
  CreateProductVariantDto,
  VariantFormInput,
} from "../../types/product.types";

interface ProductVariantsDialogProps {
  open: boolean;
  onClose: () => void;
  product: Product | null;
  onSave: (
    productId: string,
    variants: CreateProductVariantDto[]
  ) => Promise<void>;
}

export default function ProductVariantsDialog({
  open,
  onClose,
  product,
  onSave,
}: ProductVariantsDialogProps) {
  const [variants, setVariants] = useState<CreateProductVariantDto[]>([]);
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const [variantInput, setVariantInput] = useState<VariantFormInput>({
    sku: "",
    size: "",
    color: "",
    price: "",
    stockQuantity: "",
    imageUrl: "",
  });

  useEffect(() => {
    if (product && product.variants) {
      // Convert product variants to editable format
      setVariants(
        product.variants.map((v) => {
          return {
            sku: v.sku || "",
            price: v.price,
            stockQuantity: v.stockQuantity,
            attributes: v.attributes,
            imageUrl: v.imageUrl,
          };
        })
      );
    } else {
      setVariants([]);
    }
  }, [product]);

  const handleAddOrUpdateVariant = () => {
    const price =
      typeof variantInput.price === "string"
        ? parseFloat(variantInput.price)
        : variantInput.price;
    const stockQuantity =
      typeof variantInput.stockQuantity === "string"
        ? parseInt(variantInput.stockQuantity)
        : variantInput.stockQuantity;

    if (
      !variantInput.sku ||
      isNaN(price) ||
      price < 0 ||
      isNaN(stockQuantity) ||
      stockQuantity < 0
    ) {
      alert("Por favor completa SKU, Precio y Cantidad correctamente");
      return;
    }

    // Build attributes JSON
    const attributes: { size?: string; color?: string } = {};
    if (variantInput.size) attributes.size = variantInput.size;
    if (variantInput.color) attributes.color = variantInput.color;

    const newVariant: CreateProductVariantDto = {
      sku: variantInput.sku,
      price,
      stockQuantity,
      attributes:
        Object.keys(attributes).length > 0
          ? JSON.stringify(attributes)
          : undefined,
      imageUrl: variantInput.imageUrl || undefined,
    };

    if (editingIndex !== null) {
      // Update existing
      const updated = [...variants];
      updated[editingIndex] = newVariant;
      setVariants(updated);
      setEditingIndex(null);
    } else {
      // Add new
      setVariants([...variants, newVariant]);
    }

    // Reset form
    setVariantInput({
      sku: "",
      size: "",
      color: "",
      price: "",
      stockQuantity: "",
      imageUrl: "",
    });
  };

  const handleEditVariant = (index: number) => {
    const variant = variants[index];
    let attributes: { size?: string; color?: string } = {};

    if (variant.attributes) {
      try {
        attributes = JSON.parse(variant.attributes);
      } catch (e) {
        console.error("Error parsing attributes:", e);
      }
    }

    setVariantInput({
      sku: variant.sku || "",
      size: attributes.size || "",
      color: attributes.color || "",
      price: variant.price.toString(),
      stockQuantity: variant.stockQuantity.toString(),
      imageUrl: variant.imageUrl || "",
    });
    setEditingIndex(index);
  };

  const handleDeleteVariant = (index: number) => {
    const updated = variants.filter((_, i) => i !== index);
    setVariants(updated);

    if (editingIndex === index) {
      setEditingIndex(null);
      setVariantInput({
        sku: "",
        size: "",
        color: "",
        price: "",
        stockQuantity: "",
        imageUrl: "",
      });
    } else if (editingIndex !== null && editingIndex > index) {
      setEditingIndex(editingIndex - 1);
    }
  };

  const handleCancelEdit = () => {
    setEditingIndex(null);
    setVariantInput({
      sku: "",
      size: "",
      color: "",
      price: "",
      stockQuantity: "",
      imageUrl: "",
    });
  };

  const handleSave = async () => {
    if (!product) return;

    setIsSaving(true);
    try {
      await onSave(product.id, variants);
      onClose();
    } catch (error) {
      console.log(error);
      console.error("Error saving variants:", error);
      alert("Error al guardar las variantes");
    } finally {
      setIsSaving(false);
    }
  };

  const getTotalStock = () => {
    return variants.reduce((sum, v) => sum + v.stockQuantity, 0);
  };

  return (
    <Dialog open={open} onClose={onClose} className="relative z-10">
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
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
              <div className="flex justify-between items-start mb-6">
                <div>
                  <DialogTitle
                    as="h3"
                    className="text-lg leading-6 font-medium text-gray-900"
                  >
                    Variantes de Producto
                  </DialogTitle>
                  {product && (
                    <p className="mt-1 text-sm text-gray-500">
                      {product.name} • {variants.length} variante(s) • Total
                      stock: {getTotalStock()}
                    </p>
                  )}
                </div>
              </div>

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Left: Form */}
                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-4">
                    {editingIndex !== null
                      ? "Editar Variante"
                      : "Agregar Variante"}
                  </h4>

                  {editingIndex !== null && (
                    <div className="mb-3 flex items-center justify-between bg-blue-50 border border-blue-200 rounded px-3 py-2">
                      <span className="text-sm text-blue-700 font-medium">
                        Editando variante #{editingIndex + 1}
                      </span>
                      <button
                        type="button"
                        onClick={handleCancelEdit}
                        className="text-sm text-blue-600 hover:text-blue-800"
                      >
                        Cancelar
                      </button>
                    </div>
                  )}

                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
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

                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
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
                        <label className="block text-sm font-medium text-gray-700 mb-1">
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
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Precio * ($)
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

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Cantidad *
                        </label>
                        <input
                          type="number"
                          placeholder="0"
                          min="0"
                          value={variantInput.stockQuantity}
                          onChange={(e) =>
                            setVariantInput({
                              ...variantInput,
                              stockQuantity: e.target.value,
                            })
                          }
                          className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        URL de Imagen (opcional)
                      </label>
                      <input
                        type="url"
                        placeholder="https://..."
                        value={variantInput.imageUrl}
                        onChange={(e) =>
                          setVariantInput({
                            ...variantInput,
                            imageUrl: e.target.value,
                          })
                        }
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>

                    <button
                      type="button"
                      onClick={handleAddOrUpdateVariant}
                      className="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700"
                    >
                      {editingIndex !== null
                        ? "Actualizar Variante"
                        : "Agregar Variante"}
                    </button>
                  </div>
                </div>

                {/* Right: Variants List */}
                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-4">
                    Variantes ({variants.length})
                  </h4>

                  <div className="space-y-2 max-h-[500px] overflow-y-auto">
                    {variants.length > 0 ? (
                      variants.map((variant, index) => {
                        let attributes: { size?: string; color?: string } = {};
                        if (variant.attributes) {
                          try {
                            attributes = JSON.parse(variant.attributes);
                          } catch (e) {
                            console.error("Error:", e);
                          }
                        }

                        return (
                          <div
                            key={index}
                            className={`flex items-center justify-between p-3 border rounded-lg ${
                              editingIndex === index
                                ? "border-blue-400 bg-blue-50"
                                : "border-gray-200 bg-white"
                            }`}
                          >
                            <div className="flex-1">
                              <p className="text-sm font-medium text-gray-900">
                                {variant.sku}
                              </p>
                              {(attributes.size || attributes.color) && (
                                <p className="text-xs text-gray-500">
                                  {attributes.size &&
                                    `Talla: ${attributes.size}`}
                                  {attributes.size && attributes.color && " • "}
                                  {attributes.color &&
                                    `Color: ${attributes.color}`}
                                </p>
                              )}
                              <p className="text-xs text-gray-500 mt-1">
                                <span className="font-semibold">
                                  Cantidad: {variant.stockQuantity}
                                </span>
                                {" • "}
                                Precio: ${variant.price.toFixed(2)}
                              </p>
                            </div>
                            <div className="flex items-center gap-2 ml-2">
                              <button
                                type="button"
                                onClick={() => handleEditVariant(index)}
                                className="text-indigo-600 hover:text-indigo-800"
                                title="Editar"
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
                                onClick={() => handleDeleteVariant(index)}
                                className="text-red-600 hover:text-red-800"
                                title="Eliminar"
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
                        );
                      })
                    ) : (
                      <p className="text-sm text-gray-500 text-center py-8">
                        No hay variantes. Agrega la primera variante para
                        comenzar.
                      </p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
              <button
                type="button"
                onClick={handleSave}
                disabled={isSaving || variants.length === 0}
                className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
              >
                {isSaving ? "Guardando..." : "Guardar Variantes"}
              </button>
              <button
                type="button"
                onClick={onClose}
                className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
              >
                Cerrar
              </button>
            </div>
          </DialogPanel>
        </div>
      </div>
    </Dialog>
  );
}
