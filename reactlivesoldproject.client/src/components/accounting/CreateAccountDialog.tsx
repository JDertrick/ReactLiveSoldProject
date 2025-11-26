// src/components/accounting/CreateAccountDialog.tsx
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogFooter
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { useForm, Controller } from "react-hook-form";
import { CreateChartOfAccountDto } from "@/types/accounting.types";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";

const accountSchema = z.object({
    accountCode: z.string().min(1, "El código es requerido"),
    accountName: z.string().min(1, "El nombre es requerido"),
    accountType: z.enum(["Asset", "Liability", "Equity", "Revenue", "Expense"]),
    description: z.string().optional(),
});

interface CreateAccountDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (data: CreateChartOfAccountDto) => void;
    isLoading: boolean;
}

const CreateAccountDialog = ({ isOpen, onClose, onSubmit, isLoading }: CreateAccountDialogProps) => {
    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
        reset
    } = useForm<CreateChartOfAccountDto>({
        resolver: zodResolver(accountSchema),
    });

    const handleDialogClose = () => {
        if (!isLoading) {
            reset();
            onClose();
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={handleDialogClose}>
            <DialogContent className="sm:max-w-[425px]">
                <form onSubmit={handleSubmit(onSubmit)}>
                    <DialogHeader>
                        <DialogTitle>Crear Nueva Cuenta Contable</DialogTitle>
                        <DialogDescription>
                            Añada una nueva cuenta a su plan de cuentas.
                        </DialogDescription>
                    </DialogHeader>
                    <div className="grid gap-4 py-4">
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="accountCode" className="text-right">Código</Label>
                            <Input id="accountCode" {...register("accountCode")} className="col-span-3" />
                            {errors.accountCode && <p className="col-span-4 text-red-500 text-xs">{errors.accountCode.message}</p>}
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="accountName" className="text-right">Nombre</Label>
                            <Input id="accountName" {...register("accountName")} className="col-span-3" />
                            {errors.accountName && <p className="col-span-4 text-red-500 text-xs">{errors.accountName.message}</p>}
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="accountType" className="text-right">Tipo</Label>
                            <Controller
                                name="accountType"
                                control={control}
                                render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value} >
                                        <SelectTrigger className="col-span-3">
                                            <SelectValue placeholder="Seleccione un tipo" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Asset">Activo (Asset)</SelectItem>
                                            <SelectItem value="Liability">Pasivo (Liability)</SelectItem>
                                            <SelectItem value="Equity">Patrimonio (Equity)</SelectItem>
                                            <SelectItem value="Revenue">Ingreso (Revenue)</SelectItem>
                                            <SelectItem value="Expense">Gasto (Expense)</SelectItem>
                                        </SelectContent>
                                    </Select>
                                )}
                            />
                            {errors.accountType && <p className="col-span-4 text-red-500 text-xs">{errors.accountType.message}</p>}
                        </div>
                         <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="description" className="text-right">Descripción</Label>
                            <Input id="description" {...register("description")} className="col-span-3" />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={handleDialogClose}>Cancelar</Button>
                        <Button type="submit" disabled={isLoading}>{isLoading ? "Creando..." : "Crear Cuenta"}</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};

export default CreateAccountDialog;
