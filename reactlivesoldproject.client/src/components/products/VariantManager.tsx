import { useState } from "react";
import { ProductVariant } from "../../types/product.types";
import { CustomAlertDialog } from "../common/AlertDialog";
import { ImageUpload } from "@/components/ui/image-upload";
import {
  useGetProduct,
  useAddVariant,
  useUpdateVariant,
  useDeleteVariant,
  useUploadVariantImage,
} from "../../hooks/useProducts";

interface VariantManagerProps {
  productId: string;
}

const VariantManager = ({
  productId,
}: VariantManagerProps) => {
  // Obtener datos frescos del producto
  const { data: product, isLoading: isLoadingProduct } = useGetProduct(productId);
  const variants = product?.variants || [];
  const [variantInput, setVariantInput] = useState({
    sku: "",
    size: "",
    color: "",
    stock: "",
    price: "",
    wholesalePrice: "",
    isPrimary: false,
    imageUrl: "",
  });

  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [editingVariantId, setEditingVariantId] = useState<string | null>(null);
  const [imageUploadKey, setImageUploadKey] = useState<number>(0);

  // Mutations
  const addVariant = useAddVariant();
  const updateVariant = useUpdateVariant();
  const deleteVariant = useDeleteVariant();
  const uploadImage = useUploadVariantImage();

  // Dialog states
  const [alertDialog, setAlertDialog] = useState<{
    open: boolean;
    title: string;
    description: string;
  }>({
    open: false,
    title: "",
    description: "",
  });

  const handleAddOrUpdateVariant = async () => {
    const stock = parseInt(variantInput.stock) || 0;
    const price = parseFloat(variantInput.price) || 0;
    const wholesalePrice = variantInput.wholesalePrice ? parseFloat(variantInput.wholesalePrice) : undefined;

    if (!variantInput.sku) {
      setAlertDialog({
        open: true,
        title: "Error",
        description: "El SKU es requerido",
      });
      return;
    }

    try {
      // Preparar attributes JSON
      const attributes: Record<string, string> = {};
      if (variantInput.size) attributes.size = variantInput.size;
      if (variantInput.color) attributes.color = variantInput.color;

      const variantData = {
        sku: variantInput.sku,
        price,
        wholesalePrice,
        stockQuantity: stock,
        attributes:
          Object.keys(attributes).length > 0
            ? JSON.stringify(attributes)
            : undefined,
        isPrimary: variantInput.isPrimary,
      };

      if (editingVariantId) {
        // Actualizar variante existente
        await updateVariant.mutateAsync({
          variantId: editingVariantId,
          variant: variantData,
        });

        // Si hay una imagen seleccionada, subirla
        if (selectedImage) {
          await uploadImage.mutateAsync({
            variantId: editingVariantId,
            image: selectedImage,
          });
        }

        setAlertDialog({
          open: true,
          title: "Éxito",
          description: "Variante actualizada correctamente",
        });
      } else {
        // Crear nueva variante
        const result = await addVariant.mutateAsync({
          productId,
          variant: variantData,
        });

        // Si hay una imagen seleccionada y se creó la variante, subirla
        if (selectedImage && result.id) {
          await uploadImage.mutateAsync({
            variantId: result.id,
            image: selectedImage,
          });
        }
        setAlertDialog({
          open: true,
          title: "Éxito",
          description: "Variante creada correctamente",
        });
      }

      // Limpiar formulario
      setVariantInput({
        sku: "",
        size: "",
        color: "",
        stock: "",
        price: "",
        wholesalePrice: "",
        isPrimary: false,
        imageUrl: "",
      });
      setSelectedImage(null);
      setEditingVariantId(null);
      setImageUploadKey((prev) => prev + 1); // Forzar remontaje del ImageUpload
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error?.response?.data?.message || "Error al guardar la variante",
      });
    }
  };

  const handleEditVariant = (variant: ProductVariant) => {
    // Parsear attributes para obtener size y color
    let size = "";
    let color = "";
    if (variant.attributes) {
      try {
        const attrs = JSON.parse(variant.attributes);
        size = attrs.size || "";
        color = attrs.color || "";
      } catch (e) {
        console.error("Error parsing attributes:", e);
      }
    }

    setVariantInput({
      sku: variant.sku || "",
      size,
      color,
      stock: variant.stockQuantity.toString(),
      price: variant.price.toString(),
      wholesalePrice: variant.wholesalePrice?.toString() || "",
      isPrimary: variant.isPrimary || false,
      imageUrl: variant.imageUrl || "",
    });
    setSelectedImage(null); // Limpiar imagen seleccionada
    setEditingVariantId(variant.id);
    setImageUploadKey((prev) => prev + 1); // Forzar remontaje del ImageUpload
  };

  const handleCancelEdit = () => {
    setVariantInput({
      sku: "",
      size: "",
      color: "",
      stock: "",
      price: "",
      isPrimary: false,
      imageUrl: "",
    });
    setSelectedImage(null);
    setEditingVariantId(null);
    setImageUploadKey((prev) => prev + 1); // Forzar remontaje del ImageUpload
  };

  const handleDeleteVariant = async (variantId: string) => {
    if (!confirm("¿Está seguro de eliminar esta variante?")) {
      return;
    }

    try {
      await deleteVariant.mutateAsync(variantId);
      setAlertDialog({
        open: true,
        title: "Éxito",
        description: "Variante eliminada correctamente",
      });
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error?.response?.data?.message || "Error al eliminar la variante",
      });
    }
  };

  const handleImageSelect = (file: File) => {
    setSelectedImage(file);
  };

  return (
    <div>
      <h4 className="text-sm font-medium text-gray-700 mb-3">
        Variantes del Producto
      </h4>

      {/* Variant Input */}
      <div className="bg-gray-50 p-4 rounded-lg mb-4">
        {editingVariantId && (
          <div className="mb-3 flex items-center justify-between bg-blue-50 border border-blue-200 rounded px-3 py-2">
            <span className="text-sm text-blue-700 font-medium">
              Editando variante
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
              Precio (Detal) *
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
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Precio al Por Mayor (opcional)
            </label>
            <input
              type="number"
              placeholder="0.00"
              step="0.01"
              min="0"
              value={variantInput.wholesalePrice}
              onChange={(e) =>
                setVariantInput({
                  ...variantInput,
                  wholesalePrice: e.target.value,
                })
              }
              className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Stock Inicial
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
        </div>

        {/* Imagen de la variante */}
        <div className="mb-3">
          <ImageUpload
            key={imageUploadKey}
            label="Imagen de la Variante"
            currentImageUrl={variantInput.imageUrl}
            onImageSelect={handleImageSelect}
            maxSizeMB={5}
          />
          {selectedImage && (
            <p className="text-xs text-green-600 mt-1">
              Nueva imagen seleccionada: {selectedImage.name}
            </p>
          )}
          {!selectedImage && variantInput.imageUrl && (
            <p className="text-xs text-gray-600 mt-1">Imagen actual cargada</p>
          )}
        </div>

        {/* Checkbox para variante principal */}
        <div className="flex items-center mb-3">
          <input
            id="isPrimary"
            name="isPrimary"
            type="checkbox"
            checked={variantInput.isPrimary}
            onChange={(e) =>
              setVariantInput({
                ...variantInput,
                isPrimary: e.target.checked,
              })
            }
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
          />
          <label
            htmlFor="isPrimary"
            className="ml-2 block text-sm text-gray-900"
          >
            Variante Principal (su imagen se mostrará como imagen del producto)
          </label>
        </div>

        <button
          type="button"
          onClick={handleAddOrUpdateVariant}
          disabled={
            addVariant.isPending ||
            updateVariant.isPending ||
            uploadImage.isPending
          }
          className="w-full inline-flex justify-center items-center px-3 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {addVariant.isPending ||
          updateVariant.isPending ||
          uploadImage.isPending
            ? "Guardando..."
            : editingVariantId
            ? "Actualizar Variante"
            : "Agregar Variante"}
        </button>
      </div>

      {/* Variants List */}
      <div className="space-y-2 max-h-96 overflow-y-auto">
        {isLoadingProduct ? (
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
          </div>
        ) : variants && variants.length > 0 ? (
          variants.map((variant) => (
            <div
              key={variant.id}
              className={`flex items-start gap-3 p-3 bg-white border rounded-lg ${
                editingVariantId === variant.id
                  ? "border-blue-400 bg-blue-50"
                  : variant.isPrimary
                  ? "border-indigo-400 bg-indigo-50"
                  : "border-gray-200"
              }`}
            >
              {/* Imagen de la variante */}
              {variant.imageUrl && (
                <img
                  src={variant.imageUrl}
                  alt={variant.sku}
                  className="h-16 w-16 rounded-md object-cover flex-shrink-0"
                />
              )}

              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <p className="text-sm font-medium text-gray-900">
                    {variant.sku}
                  </p>
                  {variant.isPrimary && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-indigo-600 text-white">
                      Principal
                    </span>
                  )}
                </div>
                <p className="text-xs text-gray-500">
                  {(() => {
                    try {
                      if (!variant.attributes) return null;
                      const attrs = JSON.parse(variant.attributes);
                      const parts = [];
                      if (attrs.size) parts.push(`Talla: ${attrs.size}`);
                      if (attrs.color) parts.push(`Color: ${attrs.color}`);
                      return parts.join(" • ");
                    } catch {
                      return null;
                    }
                  })()}
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  <span className="font-semibold">
                    Stock: {variant.stockQuantity}
                  </span>
                  {variant.price > 0 &&
                    ` • Detal: $${variant.price.toFixed(2)}`}
                  {variant.wholesalePrice && variant.wholesalePrice > 0 &&
                    ` • Mayor: $${variant.wholesalePrice.toFixed(2)}`}
                  {` • Costo Prom: $${variant.averageCost.toFixed(2)}`}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => handleEditVariant(variant)}
                  className="text-indigo-600 hover:text-indigo-800"
                  title="Editar variante"
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
                  onClick={() => handleDeleteVariant(variant.id)}
                  disabled={deleteVariant.isPending}
                  className="text-red-600 hover:text-red-800 disabled:opacity-50"
                  title="Eliminar variante"
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
            No hay variantes agregadas aún
          </p>
        )}
      </div>

      {/* Alert Dialog */}
      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />
    </div>
  );
};

export default VariantManager;
