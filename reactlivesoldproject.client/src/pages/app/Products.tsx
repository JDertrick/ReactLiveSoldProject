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
  UpdateProductVariantDto,
} from "../../types/product.types";
import ProductFormModal from "../../components/products/ProductFormModal";
import VariantModal from "../../components/products/VariantModal";
import { CustomAlertDialog } from "@/components/common/AlertDialog";
import { AlertDialogState } from "@/types/alertdialogstate.type";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getImageUrl } from "../../utils/imageHelper";


const ProductsPage = () => {
  const { data: products, isLoading } = useGetProducts(true); // Include unpublished
  const { data: tags } = useGetTags();
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: "",
    description: "",
  });

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
    stockQuantity: variant.stockQuantity,
    imageUrl: variant.imageUrl,
  });

  // Helper: Convertir ProductVariantDto (formulario) a CreateProductVariantDto (backend)
  const convertToCreateVariantDto = (
    variant: ProductVariantDto
  ): CreateProductVariantDto => {
    const attributes: Record<string, string> = {};
    if (variant.size) attributes.size = variant.size;
    if (variant.color) attributes.color = variant.color;

    return {
      sku: variant.sku,
      price: variant.price,
      stockQuantity: 0,
      attributes:
        Object.keys(attributes).length > 0
          ? JSON.stringify(attributes)
          : undefined,
      imageUrl: variant.imageUrl,
    };
  };

  const convertToUpdateVariantDto = (
    variant: ProductVariantDto
  ): UpdateProductVariantDto => {
    const attributes: Record<string, string> = {};
    if (variant.size) attributes.size = variant.size;
    if (variant.color) attributes.color = variant.color;

    return {
      id: variant.id,
      sku: variant.sku,
      price: variant.price,
      attributes:
        Object.keys(attributes).length > 0
          ? JSON.stringify(attributes)
          : undefined,
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
          variants: variants.map(convertToUpdateVariantDto),
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
    } catch (error: any) {
      console.error("Error saving product:", error);
      setAlertDialog({
        open: true,
        title: "Error",
        description: error?.response?.data?.message || "An error occurred",
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
    <div className="container mx-auto py-6 space-y-6">
      {/* Page Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Inventario</h1>
          <p className="text-gray-500 mt-2 text-sm">
            Gestiona tu catálogo de productos, variantes y niveles de
            inventario.
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <svg
            className="w-5 h-5"
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
          Agregar producto
        </Button>
      </div>
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle>Lista de productos</CardTitle>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {/* Desktop Table View */}
          <div className="hidden md:block">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Producto</TableHead>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Variantes</TableHead>
                  <TableHead className="text-right">Precio Base</TableHead>
                  <TableHead className="text-right">Stock Total</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Etiquetas</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {products && products.length > 0 ? (
                  products.map((product) => {
                    const totalStock =
                      product.variants?.reduce(
                        (sum, v) => sum + v.stockQuantity,
                        0
                      ) || 0;
                    return (
                      <TableRow key={product.id}>
                        <TableCell>
                          <div className="flex items-center gap-3">
                            {product.imageUrl && (
                              <img
                                src={getImageUrl(product.imageUrl) || ''}
                                alt={product.name}
                                className="h-12 w-12 rounded-md object-cover"
                              />
                            )}
                            <div>
                              <div className="font-medium">{product.name}</div>
                              {product.description && (
                                <div className="text-sm text-gray-500 truncate max-w-xs">
                                  {product.description}
                                </div>
                              )}
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant="secondary">
                            {product.productType}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="space-y-2">
                            {product.variants && product.variants.length > 0 ? (
                              product.variants.map((variant) => (
                                <div
                                  key={variant.id}
                                  className="text-sm border-l-2 border-blue-400 pl-2 py-1"
                                >
                                  <div className="font-medium text-gray-900">
                                    {variant.sku}
                                  </div>
                                  <div className="flex gap-3 mt-1 text-xs">
                                    <span className="text-gray-600">
                                      Stock:{" "}
                                      <span
                                        className={`font-bold ${
                                          variant.stockQuantity < 5
                                            ? "text-red-600"
                                            : "text-green-600"
                                        }`}
                                      >
                                        {variant.stockQuantity}
                                      </span>
                                    </span>
                                    <span className="text-gray-600">
                                      Precio:{" "}
                                      <span className="font-bold text-blue-600">
                                        ${variant.price.toFixed(2)}
                                      </span>
                                    </span>
                                    <span className="text-gray-600">
                                      Costo Prom:{" "}
                                      <span className="font-bold text-orange-600">
                                        ${variant.averageCost.toFixed(2)}
                                      </span>
                                    </span>
                                  </div>
                                  {variant.attributes && (
                                    <div className="text-gray-400 mt-1 text-xs">
                                      {(() => {
                                        try {
                                          const attrs = JSON.parse(
                                            variant.attributes
                                          );
                                          return Object.entries(attrs)
                                            .map(
                                              ([key, value]) =>
                                                `${key}: ${value}`
                                            )
                                            .join(", ");
                                        } catch {
                                          return "";
                                        }
                                      })()}
                                    </div>
                                  )}
                                </div>
                              ))
                            ) : (
                              <span className="text-sm text-gray-500">
                                Sin variantes
                              </span>
                            )}
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-semibold">
                          ${(product.basePrice || 0).toFixed(2)}
                        </TableCell>
                        <TableCell className="text-right">
                          <span
                            className={`font-semibold ${
                              totalStock < 10
                                ? "text-red-600"
                                : "text-green-600"
                            }`}
                          >
                            {totalStock}
                          </span>
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant={
                              product.isPublished ? "default" : "secondary"
                            }
                          >
                            {product.isPublished ? "Publicado" : "Borrador"}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="flex flex-wrap gap-1">
                            {product.tags && product.tags.length > 0 ? (
                              product.tags.map((tag) => (
                                <Badge
                                  key={tag.id}
                                  variant="outline"
                                  className="text-xs"
                                >
                                  {tag.name}
                                </Badge>
                              ))
                            ) : (
                              <span className="text-sm text-gray-400">-</span>
                            )}
                          </div>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(product)}
                          >
                            Editar
                          </Button>
                        </TableCell>
                      </TableRow>
                    );
                  })
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={8}
                      className="text-center py-8 text-gray-500"
                    >
                      No se encontraron productos. Agrega tu primer producto para comenzar.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {/* Mobile Card View */}
          <div className="md:hidden space-y-4">
            {products && products.length > 0 ? (
              products.map((product) => {
                const totalStock =
                  product.variants?.reduce(
                    (sum, v) => sum + v.stockQuantity,
                    0
                  ) || 0;
                return (
                  <div
                    key={product.id}
                    className="border rounded-lg p-4 space-y-3 bg-white shadow-sm"
                  >
                    {/* Product Header */}
                    <div className="flex gap-3">
                      {product.imageUrl && (
                        <img
                          src={getImageUrl(product.imageUrl) || ''}
                          alt={product.name}
                          className="h-16 w-16 rounded-md object-cover flex-shrink-0"
                        />
                      )}
                      <div className="flex-1 min-w-0">
                        <div className="font-medium text-lg truncate">
                          {product.name}
                        </div>
                        {product.description && (
                          <div className="text-sm text-gray-500 line-clamp-2">
                            {product.description}
                          </div>
                        )}
                      </div>
                    </div>

                    {/* Status and Type */}
                    <div className="flex gap-2 flex-wrap">
                      <Badge variant="secondary">{product.productType}</Badge>
                      <Badge
                        variant={product.isPublished ? "default" : "secondary"}
                      >
                        {product.isPublished ? "Publicado" : "Borrador"}
                      </Badge>
                    </div>

                    {/* Price and Stock */}
                    <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
                      <div>
                        <div className="text-muted-foreground">Precio Base</div>
                        <div className="font-semibold">
                          ${(product.basePrice || 0).toFixed(2)}
                        </div>
                      </div>
                      <div>
                        <div className="text-muted-foreground">Stock Total</div>
                        <div
                          className={`font-semibold ${
                            totalStock < 10 ? "text-red-600" : "text-green-600"
                          }`}
                        >
                          {totalStock}
                        </div>
                      </div>
                    </div>

                    {/* Variants */}
                    {product.variants && product.variants.length > 0 && (
                      <div className="border-t pt-3">
                        <div className="text-sm font-medium mb-2">Variantes</div>
                        <div className="space-y-2">
                          {product.variants.map((variant) => (
                            <div
                              key={variant.id}
                              className="text-sm bg-gray-50 rounded p-2"
                            >
                              <div className="font-medium">{variant.sku}</div>
                              <div className="grid grid-cols-3 gap-2 mt-1 text-xs">
                                <div>
                                  <span className="text-gray-600">Stock: </span>
                                  <span
                                    className={`font-bold ${
                                      variant.stockQuantity < 5
                                        ? "text-red-600"
                                        : "text-green-600"
                                    }`}
                                  >
                                    {variant.stockQuantity}
                                  </span>
                                </div>
                                <div>
                                  <span className="text-gray-600">Precio: </span>
                                  <span className="font-bold">
                                    ${variant.price.toFixed(2)}
                                  </span>
                                </div>
                                <div>
                                  <span className="text-gray-600">Costo: </span>
                                  <span className="font-bold">
                                    ${variant.averageCost.toFixed(2)}
                                  </span>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Tags */}
                    {product.tags && product.tags.length > 0 && (
                      <div className="border-t pt-3">
                        <div className="flex flex-wrap gap-1">
                          {product.tags.map((tag) => (
                            <Badge
                              key={tag.id}
                              variant="outline"
                              className="text-xs"
                            >
                              {tag.name}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="border-t pt-3">
                      <Button
                        variant="outline"
                        size="sm"
                        className="w-full"
                        onClick={() => handleOpenModal(product)}
                      >
                        Editar Producto
                      </Button>
                    </div>
                  </div>
                );
              })
            ) : (
              <div className="text-center py-8 text-gray-500 border rounded-lg">
                No se encontraron productos. Agrega tu primer producto para comenzar.
              </div>
            )}
          </div>
        </CardContent>
      </Card>

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

      {/* Dialog */}
      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />

      {/* Variant Management Modal */}
      <VariantModal
        isOpen={isVariantModalOpen}
        productName={editingProduct?.name || "Nuevo Producto"}
        variants={variants}
        onClose={handleCloseVariantModal}
        onSaveVariants={handleVariantsChange}
        customAlertDialog={setAlertDialog}
      />
    </div>
  );
};

export default ProductsPage;
