import { useState, useMemo } from 'react';
import { useNoSeries } from '../../hooks/useNoSeries';
import { NoSerie, DocumentType, DocumentTypeLabels } from '../../types/noserie.types';
import { Button } from '../../components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../../components/ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '../../components/ui/alert-dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { toast } from 'sonner';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Search, Plus, Edit, Trash2, CheckCircle, XCircle, List } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import NoSerieFormDialog from './components/NoSerieFormDialog';
import NoSerieDetailDialog from './components/NoSerieDetailDialog';
import NumberGeneratorTester from './components/NumberGeneratorTester';

const NoSeriesPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const { series, isLoading, error, deleteSerie } = useNoSeries();

  const [isFormDialogOpen, setIsFormDialogOpen] = useState(false);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedSerie, setSelectedSerie] = useState<NoSerie | null>(null);
  const [serieToDelete, setSerieToDelete] = useState<NoSerie | null>(null);

  const filteredSeries = useMemo(() => {
    if (!searchTerm || !series) return series;

    return series.filter(
      (s) =>
        s.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.documentTypeName?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [series, searchTerm]);

  const handleCreate = () => {
    setSelectedSerie(null);
    setIsFormDialogOpen(true);
  };

  const handleEdit = (serie: NoSerie) => {
    setSelectedSerie(serie);
    setIsFormDialogOpen(true);
  };

  const handleViewDetail = (serie: NoSerie) => {
    setSelectedSerie(serie);
    setIsDetailDialogOpen(true);
  };

  const handleDeleteClick = (serie: NoSerie) => {
    setSerieToDelete(serie);
    setIsDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!serieToDelete) return;

    try {
      await deleteSerie(serieToDelete.id);
      toast.success('Serie eliminada exitosamente');
      setIsDeleteDialogOpen(false);
      setSerieToDelete(null);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Error al eliminar la serie');
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-gray-500">Cargando series numéricas...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-red-500">Error al cargar las series: {error.message}</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Series Numéricas</CardTitle>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="series" className="w-full">
            <TabsList className="mb-4">
              <TabsTrigger value="series">Gestión de Series</TabsTrigger>
              <TabsTrigger value="tester">Generador de Números</TabsTrigger>
            </TabsList>

            <TabsContent value="series" className="space-y-4">
              <div className="flex justify-end mb-4">
                <Button onClick={handleCreate}>
                  <Plus className="mr-2 h-4 w-4" />
                  Nueva Serie
                </Button>
              </div>
          {/* Search Bar */}
          <div className="mb-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                type="text"
                placeholder="Buscar por código, descripción o tipo..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>

          {/* Table */}
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Código</TableHead>
                  <TableHead>Descripción</TableHead>
                  <TableHead>Tipo de Documento</TableHead>
                  <TableHead className="text-center">Líneas</TableHead>
                  <TableHead className="text-center">Por Defecto</TableHead>
                  <TableHead className="text-center">Manual</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredSeries && filteredSeries.length > 0 ? (
                  filteredSeries.map((serie) => (
                    <TableRow key={serie.id}>
                      <TableCell className="font-medium">{serie.code}</TableCell>
                      <TableCell>{serie.description}</TableCell>
                      <TableCell>
                        {serie.documentType !== undefined && serie.documentType !== null ? (
                          <Badge variant="outline">
                            {DocumentTypeLabels[serie.documentType as DocumentType]}
                          </Badge>
                        ) : (
                          <span className="text-gray-400">Sin tipo</span>
                        )}
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge variant="secondary">{serie.noSerieLines?.length || 0}</Badge>
                      </TableCell>
                      <TableCell className="text-center">
                        {serie.defaultNos ? (
                          <CheckCircle className="h-5 w-5 text-green-500 mx-auto" />
                        ) : (
                          <XCircle className="h-5 w-5 text-gray-300 mx-auto" />
                        )}
                      </TableCell>
                      <TableCell className="text-center">
                        {serie.manualNos ? (
                          <CheckCircle className="h-5 w-5 text-green-500 mx-auto" />
                        ) : (
                          <XCircle className="h-5 w-5 text-gray-300 mx-auto" />
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleViewDetail(serie)}
                          >
                            <List className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="sm" onClick={() => handleEdit(serie)}>
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteClick(serie)}
                          >
                            <Trash2 className="h-4 w-4 text-red-500" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-8 text-gray-500">
                      {searchTerm
                        ? 'No se encontraron series con ese criterio'
                        : 'No hay series numéricas configuradas'}
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
            </TabsContent>

            <TabsContent value="tester">
              <NumberGeneratorTester />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Form Dialog */}
      <NoSerieFormDialog
        open={isFormDialogOpen}
        onOpenChange={setIsFormDialogOpen}
        serie={selectedSerie}
        onSuccess={() => {
          setIsFormDialogOpen(false);
          setSelectedSerie(null);
        }}
      />

      {/* Detail Dialog */}
      <NoSerieDetailDialog
        open={isDetailDialogOpen}
        onOpenChange={setIsDetailDialogOpen}
        serie={selectedSerie}
        onEdit={(serie) => {
          setIsDetailDialogOpen(false);
          handleEdit(serie);
        }}
      />

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Estás seguro?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción eliminará la serie numérica "{serieToDelete?.code}" y todas sus líneas.
              Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteConfirm} className="bg-red-500 hover:bg-red-600">
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default NoSeriesPage;
