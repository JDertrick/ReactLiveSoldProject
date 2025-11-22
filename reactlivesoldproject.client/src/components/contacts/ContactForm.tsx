import { useForm } from 'react-hook-form';
import { Contact, CreateContactDto, UpdateContactDto } from '../../types/contact.types';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Textarea } from '../ui/textarea';
import { Switch } from '../ui/switch';

interface ContactFormProps {
  contact?: Contact;
  onSubmit: (data: CreateContactDto | UpdateContactDto) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function ContactForm({ contact, onSubmit, onCancel, isLoading }: ContactFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<CreateContactDto | UpdateContactDto>({
    defaultValues: contact || {
      email: '',
      firstName: '',
      lastName: '',
      phone: '',
      address: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      company: '',
      jobTitle: '',
      isActive: true,
    },
  });

  const isActive = watch('isActive');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Información básica */}
      <div className="space-y-4">
        <h3 className="text-lg font-medium">Información de Contacto</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="firstName">Nombre</Label>
            <Input
              id="firstName"
              {...register('firstName')}
              placeholder="Nombre"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="lastName">Apellido</Label>
            <Input
              id="lastName"
              {...register('lastName')}
              placeholder="Apellido"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email *</Label>
            <Input
              id="email"
              type="email"
              {...register('email', {
                required: !contact ? 'El email es obligatorio' : false
              })}
              placeholder="email@ejemplo.com"
            />
            {errors.email && (
              <p className="text-sm text-red-500">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="phone">Teléfono</Label>
            <Input
              id="phone"
              type="tel"
              {...register('phone')}
              placeholder="+1234567890"
            />
          </div>
        </div>
      </div>

      {/* Información empresarial */}
      <div className="space-y-4">
        <h3 className="text-lg font-medium">Información Empresarial</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="company">Empresa</Label>
            <Input
              id="company"
              {...register('company')}
              placeholder="Nombre de la empresa"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="jobTitle">Puesto</Label>
            <Input
              id="jobTitle"
              {...register('jobTitle')}
              placeholder="Gerente, Director, etc."
            />
          </div>
        </div>
      </div>

      {/* Dirección */}
      <div className="space-y-4">
        <h3 className="text-lg font-medium">Dirección</h3>

        <div className="space-y-2">
          <Label htmlFor="address">Dirección</Label>
          <Textarea
            id="address"
            {...register('address')}
            placeholder="Calle, número, departamento..."
            rows={2}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="city">Ciudad</Label>
            <Input
              id="city"
              {...register('city')}
              placeholder="Ciudad"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="state">Estado/Provincia</Label>
            <Input
              id="state"
              {...register('state')}
              placeholder="Estado o provincia"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="postalCode">Código Postal</Label>
            <Input
              id="postalCode"
              {...register('postalCode')}
              placeholder="12345"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="country">País</Label>
            <Input
              id="country"
              {...register('country')}
              placeholder="País"
            />
          </div>
        </div>
      </div>

      {/* Estado */}
      <div className="flex items-center space-x-2">
        <Switch
          id="isActive"
          checked={isActive}
          onCheckedChange={(checked) => setValue('isActive', checked)}
        />
        <Label htmlFor="isActive">Contacto activo</Label>
      </div>

      {/* Botones de acción */}
      <div className="flex justify-end gap-4">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? 'Guardando...' : contact ? 'Actualizar' : 'Crear'}
        </Button>
      </div>
    </form>
  );
}
