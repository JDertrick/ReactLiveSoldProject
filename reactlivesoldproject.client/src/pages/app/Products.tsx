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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

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
      stockQuantity: variant.stock,
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
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle>Products</CardTitle>
              <CardDescription>
                Manage your product catalog, variants, and inventory levels
              </CardDescription>
            </div>
            <Button onClick={() => handleOpenModal()}>
              Add Product
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Variants</TableHead>
                <TableHead className="text-right">Base Price</TableHead>
                <TableHead className="text-right">Total Stock</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Tags</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {products && products.length > 0 ? (
                products.map((product) => {
                  const totalStock =
                    product.variants?.reduce((sum, v) => sum + v.stockQuantity, 0) || 0;
                  return (
                    <TableRow key={product.id}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          {product.imageUrl && (
                            <img
                              src={product.imageUrl}
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
                        <Badge variant="secondary">{product.productType}</Badge>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          {product.variants && product.variants.length > 0 ? (
                            product.variants.map((variant) => (
                              <div key={variant.id} className="text-sm">
                                <span className="font-medium">{variant.sku}</span>
                                <span className="text-gray-500 ml-2">
                                  Stock: <span className={`font-semibold ${variant.stockQuantity < 5 ? 'text-red-600' : 'text-green-600'}`}>
                                    {variant.stockQuantity}
                                  </span>
                                </span>
                                {variant.attributes && (
                                  <span className="text-gray-400 ml-2">
                                    {(() => {
                                      try {
                                        const attrs = JSON.parse(variant.attributes);
                                        return Object.entries(attrs)
                                          .map(([key, value]) => `${key}: ${value}`)
                                          .join(", ");
                                      } catch {
                                        return "";
                                      }
                                    })()}
                                  </span>
                                )}
                              </div>
                            ))
                          ) : (
                            <span className="text-sm text-gray-500">No variants</span>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right font-semibold">
                        ${(product.basePrice || 0).toFixed(2)}
                      </TableCell>
                      <TableCell className="text-right">
                        <span className={`font-semibold ${totalStock < 10 ? 'text-red-600' : 'text-green-600'}`}>
                          {totalStock}
                        </span>
                      </TableCell>
                      <TableCell>
                        <Badge variant={product.isPublished ? "default" : "secondary"}>
                          {product.isPublished ? "Published" : "Draft"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {product.tags && product.tags.length > 0 ? (
                            product.tags.map((tag) => (
                              <Badge key={tag.id} variant="outline" className="text-xs">
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
                          Edit
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })
              ) : (
                <TableRow>
                  <TableCell colSpan={8} className="text-center py-8 text-gray-500">
                    No products found. Add your first product to get started.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
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
        productName={editingProduct?.name || "New Product"}
        variants={variants}
        onClose={handleCloseVariantModal}
        onSaveVariants={handleVariantsChange}
        customAlertDialog={setAlertDialog}
      />
    </div>
  );
};

export default ProductsPage;
