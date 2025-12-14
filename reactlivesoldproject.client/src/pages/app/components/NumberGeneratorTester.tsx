import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { useNoSeries } from '../../../hooks/useNoSeries';
import { DocumentType, DocumentTypeLabels } from '../../../types/noserie.types';
import { Copy } from 'lucide-react';

const NumberGeneratorTester = () => {
  const { getNextNumber, getNextNumberByType, validateNumber } = useNoSeries();

  const [testMode, setTestMode] = useState<'code' | 'type'>('code');
  const [serieCode, setSerieCode] = useState('');
  const [documentType, setDocumentType] = useState('');
  const [generatedNumber, setGeneratedNumber] = useState('');
  const [isGenerating, setIsGenerating] = useState(false);

  const [validateSerieCode, setValidateSerieCode] = useState('');
  const [numberToValidate, setNumberToValidate] = useState('');
  const [validationResult, setValidationResult] = useState<boolean | null>(null);
  const [isValidating, setIsValidating] = useState(false);

  const handleGenerateNumber = async () => {
    setIsGenerating(true);
    try {
      let result;
      if (testMode === 'code') {
        if (!serieCode) {
          toast.error('Ingresa un código de serie');
          return;
        }
        result = await getNextNumber(serieCode);
      } else {
        if (!documentType) {
          toast.error('Selecciona un tipo de documento');
          return;
        }
        result = await getNextNumberByType(parseInt(documentType));
      }

      setGeneratedNumber(result.nextNumber);
      toast.success(`Número generado: ${result.nextNumber}`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Error al generar número');
      setGeneratedNumber('');
    } finally {
      setIsGenerating(false);
    }
  };

  const handleValidateNumber = async () => {
    if (!validateSerieCode || !numberToValidate) {
      toast.error('Completa todos los campos');
      return;
    }

    setIsValidating(true);
    try {
      const result = await validateNumber(validateSerieCode, numberToValidate);
      setValidationResult(result.isValid);
      toast.success(result.isValid ? 'Número válido' : 'Número inválido');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Error al validar número');
      setValidationResult(null);
    } finally {
      setIsValidating(false);
    }
  };

  const copyToClipboard = () => {
    navigator.clipboard.writeText(generatedNumber);
    toast.success('Número copiado al portapapeles');
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Number Generator */}
      <Card>
        <CardHeader>
          <CardTitle>Generar Siguiente Número</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <Label>Modo de Generación</Label>
            <Select
              value={testMode}
              onValueChange={(value: 'code' | 'type') => {
                setTestMode(value);
                setGeneratedNumber('');
              }}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="code">Por Código de Serie</SelectItem>
                <SelectItem value="type">Por Tipo de Documento</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {testMode === 'code' ? (
            <div>
              <Label htmlFor="serieCode">Código de Serie</Label>
              <Input
                id="serieCode"
                value={serieCode}
                onChange={(e) => setSerieCode(e.target.value.toUpperCase())}
                placeholder="Ej: CUST, INV"
              />
            </div>
          ) : (
            <div>
              <Label htmlFor="documentType">Tipo de Documento</Label>
              <Select value={documentType} onValueChange={setDocumentType}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar tipo..." />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(DocumentTypeLabels).map(([key, label]) => (
                    <SelectItem key={key} value={key}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          <Button onClick={handleGenerateNumber} disabled={isGenerating} className="w-full">
            {isGenerating ? 'Generando...' : 'Generar Número'}
          </Button>

          {generatedNumber && (
            <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg">
              <div className="flex justify-between items-center">
                <div>
                  <p className="text-sm text-gray-600">Número Generado:</p>
                  <p className="text-2xl font-mono font-bold text-green-700">
                    {generatedNumber}
                  </p>
                </div>
                <Button variant="outline" size="sm" onClick={copyToClipboard}>
                  <Copy className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Number Validator */}
      <Card>
        <CardHeader>
          <CardTitle>Validar Número</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <Label htmlFor="validateSerieCode">Código de Serie</Label>
            <Input
              id="validateSerieCode"
              value={validateSerieCode}
              onChange={(e) => setValidateSerieCode(e.target.value.toUpperCase())}
              placeholder="Ej: CUST, INV"
            />
          </div>

          <div>
            <Label htmlFor="numberToValidate">Número a Validar</Label>
            <Input
              id="numberToValidate"
              value={numberToValidate}
              onChange={(e) => setNumberToValidate(e.target.value)}
              placeholder="Ej: CUST-0001"
            />
          </div>

          <Button onClick={handleValidateNumber} disabled={isValidating} className="w-full">
            {isValidating ? 'Validando...' : 'Validar'}
          </Button>

          {validationResult !== null && (
            <div
              className={`mt-4 p-4 rounded-lg ${
                validationResult
                  ? 'bg-green-50 border border-green-200'
                  : 'bg-red-50 border border-red-200'
              }`}
            >
              <p
                className={`text-sm ${
                  validationResult ? 'text-green-700' : 'text-red-700'
                }`}
              >
                {validationResult
                  ? '✓ El número es válido para esta serie'
                  : '✗ El número no es válido para esta serie'}
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default NumberGeneratorTester;
