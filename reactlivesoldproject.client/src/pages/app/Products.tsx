import { useState, useMemo } from "react";
import {
  useGetProducts,
  useCreateProduct,
  useUpdateProduct,
  useGetStockByLocation,
} from "../../hooks/useProducts";
import {
  CreateProductDto,
  UpdateProductDto,
  Product,
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
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { getImageUrl } from "../../utils/imageHelper";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useDebounce } from "@/hooks/useDebounce";
import { toast } from "sonner";
import { MapPin, ChevronDown, Package } from "lucide-react";
import { formatCurrency } from "@/utils/currencyHelper";

// Componente para mostrar stock con detalles por ubicación
const StockDisplay = ({ stock, sku, variantId }: { stock: number; sku?: string; variantId?: string }) => {
  const [isOpen, setIsOpen] = useState(false);

  // Obtener stock por ubicación del backend
  const { data: stockByLocation, isLoading } = useGetStockByLocation(variantId, isOpen);

  const hasMultipleLocations = stock > 0 && stockByLocation && stockByLocation.length > 0;

  if (!hasMultipleLocations) {
    return (
      <div className="flex items-center gap-2">
        <Badge
          variant={stock > 10 ? "default" : stock > 0 ? "secondary" : "destructive"}
          className="font-mono"
        >
          {stock}
        </Badge>
      </div>
    );
  }

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" className="h-auto p-0 hover:bg-transparent">
          <div className="flex items-center gap-2">
            <Badge
              variant={stock > 10 ? "default" : stock > 0 ? "secondary" : "destructive"}
              className="font-mono"
            >
              {stock}
            </Badge>
            <ChevronDown className="h-3 w-3 text-muted-foreground" />
          </div>
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80" align="start">
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="font-semibold text-sm">Stock por Ubicación</h4>
            <Badge variant="outline" className="font-mono">
              Total: {stock}
            </Badge>
          </div>
          <div className="space-y-2">
            {isLoading ? (
              <div className="flex items-center justify-center p-4">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-indigo-600"></div>
              </div>
            ) : stockByLocation && stockByLocation.length > 0 ? (
              stockByLocation.map((location) => (
                <div
                  key={location.locationId}
                  className="flex items-center justify-between p-2 rounded-md bg-muted/50 hover:bg-muted transition-colors"
                >
                  <div className="flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-muted-foreground" />
                    <span className="text-sm">{location.locationName}</span>
                  </div>
                  <Badge variant="secondary" className="font-mono">
                    {location.quantity}
                  </Badge>
                </div>
              ))
            ) : (
              <div className="text-sm text-muted-foreground text-center p-4">
                No hay stock distribuido en ubicaciones
              </div>
            )}
          </div>
          {sku && (
            <div className="pt-2 border-t text-xs text-muted-foreground">
              SKU: {sku}
            </div>
          )}
        </div>
      </PopoverContent>
    </Popover>
  );
};

const ProductsPage = () => {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [status, setStatus] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { data: pagedResult, isLoading } = useGetProducts(
    page,
    pageSize,
    status,
    debouncedSearchTerm
  );
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

  const variantProducts = useMemo(
    () => pagedResult?.items ?? [],
    [pagedResult]
  );
  const totalItems = useMemo(() => pagedResult?.totalItems ?? 0, [pagedResult]);
  const totalPages = useMemo(() => pagedResult?.totalPages ?? 0, [pagedResult]);

  const handleOpenModal = (product?: Product) => {
    setEditingProduct(product || null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingProduct(null);
  };

  const handleCloseVariantModal = () => {
    setIsVariantModalOpen(false);
    setEditingProduct(null);
  };

  const handleSubmit = async (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => {
    try {
      if (isEditing && editingProduct) {
        await updateProduct.mutateAsync({
          id: editingProduct.id,
          data: data as UpdateProductDto,
        });
      } else {
        await createProduct.mutateAsync(data as CreateProductDto);
      }
      toast.success("Producto creado correctamente.");
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

  const startItem = (page - 1) * pageSize + 1;
  const endItem = Math.min(page * pageSize, totalItems);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Inventario</h1>
          <p className="text-muted-foreground mt-1">
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
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 6v6m0 0v6m0-6h6m-6 0H6"
            />
          </svg>
          Agregar producto
        </Button>
      </div>
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4 w-full">
              <Input
                placeholder="Buscar por producto o SKU..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Tabs value={status} onValueChange={setStatus}>
                <TabsList>
                  <TabsTrigger value="all">Todos</TabsTrigger>
                  <TabsTrigger value="published">Publicado</TabsTrigger>
                  <TabsTrigger value="draft">Borrador</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="hidden md:block">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[40px]">
                    <Checkbox />
                  </TableHead>
                  <TableHead>No. Producto</TableHead>
                  <TableHead>Producto</TableHead>
                  <TableHead>SKU</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Stock</TableHead>
                  <TableHead>Precios</TableHead>
                  <TableHead>Costo</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center py-8">
                      <div className="flex items-center justify-center h-64">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : variantProducts && variantProducts.length > 0 ? (
                  variantProducts.map((variant) => {
                    const hasNoVariants =
                      variant.id === "00000000-0000-0000-0000-000000000000";
                    return (
                      <TableRow key={`${variant.productId}-${variant.id}`}>
                        <TableCell>
                          <Checkbox />
                        </TableCell>
                        <TableCell>
                          <div className="text-sm font-mono text-muted-foreground">
                            {variant.product?.productNo || "-"}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <img
                              src={
                                getImageUrl(variant.imageUrl) ||
                                "/placeholder.svg"
                              }
                              alt={variant.productName}
                              className="h-10 w-10 rounded-md object-cover"
                            />
                            <div>
                              <div className="font-medium">
                                {variant.productName}
                                {hasNoVariants && (
                                  <span className="ml-2 text-xs text-orange-600">
                                    (Sin variantes)
                                  </span>
                                )}
                              </div>
                              <div className="text-sm text-gray-500">
                                {variant.productDescription}
                              </div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          {hasNoVariants ? (
                            <span className="text-gray-400 text-sm">-</span>
                          ) : (
                            variant.sku
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant={
                              variant.isPublished ? "default" : "secondary"
                            }
                          >
                            {variant.isPublished ? "Publicado" : "Borrador"}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {hasNoVariants ? (
                            <span className="text-gray-400 text-sm">-</span>
                          ) : (
                            <StockDisplay
                              stock={variant.stockQuantity}
                              sku={variant.sku}
                              variantId={variant.id}
                            />
                          )}
                        </TableCell>
                        <TableCell>
                          <div className="text-sm">
                            <div>Detal: ${formatCurrency(variant.price || 0)}</div>
                            {variant.wholesalePrice && variant.wholesalePrice > 0 && (
                              <div className="text-gray-600">
                                Mayor: {formatCurrency(variant.wholesalePrice || 0)}
                              </div>
                            )}
                          </div>
                        </TableCell>
                        <TableCell className="text-sm">
                          {variant.averageCost && variant.averageCost > 0 && (
                            <div className="text-gray-800">
                              {formatCurrency(variant.averageCost || 0)}
                            </div>
                          )}
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex gap-2 justify-end">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleOpenModal(variant.product)}
                            >
                              Editar
                            </Button>
                            <Button
                              variant={hasNoVariants ? "default" : "outline"}
                              size="sm"
                              onClick={() => {
                                setEditingProduct(variant.product);
                                setIsVariantModalOpen(true);
                              }}
                            >
                              {hasNoVariants
                                ? "Agregar variantes"
                                : "Variantes"}
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    );
                  })
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={9}
                      className="text-center py-8 text-gray-500"
                    >
                      No se encontraron productos.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
          <div className="flex justify-between items-center mt-4">
            <div className="text-sm text-gray-500">
              Mostrando {startItem} a {endItem} de {totalItems} resultados
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                Anterior
              </Button>
              <span>
                Página {page} de {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                Siguiente
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <ProductFormModal
        isOpen={isModalOpen}
        editingProduct={editingProduct}
        isLoading={createProduct.isPending || updateProduct.isPending}
        onClose={handleCloseModal}
        onSubmit={handleSubmit}
      />
      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />
      <VariantModal
        isOpen={isVariantModalOpen}
        product={editingProduct}
        onClose={handleCloseVariantModal}
        customAlertDialog={setAlertDialog}
      />
    </div>
  );
};

export default ProductsPage;
