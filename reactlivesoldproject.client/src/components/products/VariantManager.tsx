import { useState } from "react";
import { ProductVariantDto } from "../../types/product.types";
import { CustomAlertDialog } from "../common/AlertDialog";

interface VariantManagerProps {
  variants: ProductVariantDto[];
  onVariantsChange: (variants: ProductVariantDto[]) => void;
}

const VariantManager = ({
  variants,
  onVariantsChange,
}: VariantManagerProps) => {
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

  const handleAddVariant = () => {
    const stock = parseInt(variantInput.stock) || 0;
    const price = parseFloat(variantInput.price) || 0;

    if (variantInput.sku && stock >= 0) {
      if (editingVariantIndex !== null) {
        // Estamos editando una variante existente - preservar el id
        const existingVariant = variants[editingVariantIndex];
        const updatedVariant: ProductVariantDto = {
          id: existingVariant.id, // Preservar el id si existe
          sku: variantInput.sku,
          size: variantInput.size || undefined,
          color: variantInput.color || undefined,
          stock,
          stockQuantity: stock,
          price,
        };

        const newVariants = [...variants];
        newVariants[editingVariantIndex] = updatedVariant;
        onVariantsChange(newVariants);
        setEditingVariantIndex(null);
      } else {
        const existsVariant = variants.find(
          (variant) => variant.sku === variantInput.sku
        );
        if (existsVariant !== undefined) {
          setAlertDialog({
            description: "El SKU ingresado ya existe",
            open: true,
            title: "SKU existente",
          });
        } else {
          // Agregar nueva variante (sin id)
          const newVariant: ProductVariantDto = {
            sku: variantInput.sku,
            size: variantInput.size || undefined,
            color: variantInput.color || undefined,
            stock,
            stockQuantity: stock,
            price,
          };
          onVariantsChange([...variants, newVariant]);
          setVariantInput({
            sku: "",
            size: "",
            color: "",
            stock: "",
            price: "",
          });
        }
      }
    }
  };

  const handleEditVariant = (index: number) => {
    const variant = variants[index];
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
    const newVariants = [...variants];
    newVariants.splice(index, 1);
    onVariantsChange(newVariants);

    // Si estábamos editando esta variante, cancelar la edición
    if (editingVariantIndex === index) {
      handleCancelEditVariant();
    } else if (editingVariantIndex !== null && editingVariantIndex > index) {
      // Ajustar el índice si eliminamos una variante antes de la que estamos editando
      setEditingVariantIndex(editingVariantIndex - 1);
    }
  };

  return (
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
          {editingVariantIndex !== null ? "Update Variant" : "Add Variant"}
        </button>
      </div>

      {/* Variants List */}
      <div className="space-y-2 max-h-96 overflow-y-auto">
        {variants && variants.length > 0 ? (
          variants.map((variant, index) => (
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
                    ` • Precio: $${(variant.price || 0).toFixed(2)}`}
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
