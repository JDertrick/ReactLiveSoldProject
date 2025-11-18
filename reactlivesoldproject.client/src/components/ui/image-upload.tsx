import React, { useState, useRef, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Upload, X, Image as ImageIcon } from 'lucide-react';
import { getImageUrl } from '@/utils/imageHelper';

interface ImageUploadProps {
  label?: string;
  currentImageUrl?: string | null;
  onImageSelect: (file: File) => void;
  onImageRemove?: () => void;
  maxSizeMB?: number;
  acceptedFormats?: string[];
  className?: string;
}

export function ImageUpload({
  label = 'Subir imagen',
  currentImageUrl,
  onImageSelect,
  onImageRemove,
  maxSizeMB = 5,
  acceptedFormats = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'],
  className = ''
}: ImageUploadProps) {
  const [previewUrl, setPreviewUrl] = useState<string | null>(currentImageUrl || null);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Actualizar previewUrl cuando currentImageUrl cambie (después de subir al backend)
  useEffect(() => {
    if (currentImageUrl && !previewUrl?.startsWith('data:')) {
      // Solo actualizar si no es una preview local (base64)
      setPreviewUrl(currentImageUrl);
    }
  }, [currentImageUrl]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];

    if (!file) return;

    // Validar tipo de archivo
    if (!acceptedFormats.includes(file.type)) {
      setError(`Formato no válido. Formatos aceptados: ${acceptedFormats.join(', ')}`);
      return;
    }

    // Validar tamaño
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      setError(`El archivo excede el tamaño máximo de ${maxSizeMB} MB`);
      return;
    }

    // Limpiar error
    setError(null);

    // Crear preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreviewUrl(reader.result as string);
    };
    reader.readAsDataURL(file);

    // Notificar al padre
    onImageSelect(file);
  };

  const handleRemove = () => {
    setPreviewUrl(null);
    setError(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onImageRemove?.();
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  const displayUrl = previewUrl || getImageUrl(currentImageUrl);

  return (
    <div className={`space-y-2 ${className}`}>
      {label && <Label>{label}</Label>}

      <div className="flex flex-col gap-4">
        {/* Preview de la imagen */}
        {displayUrl && (
          <div className="relative w-full max-w-md h-48 border-2 border-dashed border-gray-300 rounded-lg overflow-hidden bg-gray-50">
            <img
              src={displayUrl}
              alt="Preview"
              className="w-full h-full object-contain"
            />
            <Button
              type="button"
              variant="destructive"
              size="icon"
              className="absolute top-2 right-2"
              onClick={handleRemove}
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        )}

        {/* Botón de subida */}
        {!displayUrl && (
          <div
            className="w-full max-w-md h-48 border-2 border-dashed border-gray-300 rounded-lg flex flex-col items-center justify-center gap-2 bg-gray-50 hover:bg-gray-100 cursor-pointer transition-colors"
            onClick={handleButtonClick}
          >
            <ImageIcon className="h-12 w-12 text-gray-400" />
            <Button type="button" variant="outline" size="sm">
              <Upload className="mr-2 h-4 w-4" />
              Seleccionar imagen
            </Button>
            <p className="text-xs text-gray-500">
              Máx. {maxSizeMB} MB - JPG, PNG, GIF, WEBP
            </p>
          </div>
        )}

        {/* Input file oculto */}
        <Input
          ref={fileInputRef}
          type="file"
          accept={acceptedFormats.join(',')}
          onChange={handleFileChange}
          className="hidden"
        />

        {/* Botón para cambiar imagen si ya hay una */}
        {displayUrl && (
          <Button
            type="button"
            variant="outline"
            onClick={handleButtonClick}
            className="w-full max-w-md"
          >
            <Upload className="mr-2 h-4 w-4" />
            Cambiar imagen
          </Button>
        )}

        {/* Mensaje de error */}
        {error && (
          <p className="text-sm text-red-500">{error}</p>
        )}
      </div>
    </div>
  );
}
