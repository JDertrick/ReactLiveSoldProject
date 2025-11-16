import { useState, Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { Plus, X, Trash2, DollarSign } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { CreateReceiptDto, CreateReceiptItemDto } from '../../types/wallet.types';
import { Customer } from '../../types/customer.types';
import { useCreateReceipt } from '../../hooks/useWallet';
import { toast } from 'sonner';

interface CreateReceiptModalProps {
  isOpen: boolean;
  onClose: () => void;
  customer: Customer;
}

export const CreateReceiptModal = ({ isOpen, onClose, customer }: CreateReceiptModalProps) => {
  const [receiptType, setReceiptType] = useState<'Deposit' | 'Withdrawal'>('Deposit');
  const [notes, setNotes] = useState('');
  const [items, setItems] = useState<CreateReceiptItemDto[]>([{ description: '', unitPrice: 0, quantity: 1 }]);

  const createReceiptMutation = useCreateReceipt();

  const handleAddItem = () => {
    setItems([...items, { description: '', unitPrice: 0, quantity: 1 }]);
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: keyof CreateReceiptItemDto, value: string | number) => {
    const newItems = [...items];
    if (field === 'quantity' || field === 'unitPrice') {
      newItems[index][field] = Number(value);
    } else {
      newItems[index][field] = value as any;
    }
    setItems(newItems);
  };

  const calculateTotal = () => {
    return items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (items.some(item => !item.description || item.unitPrice <= 0 || item.quantity <= 0)) {
      toast.error('Please ensure all receipt items have a description, positive unit price, and positive quantity.');
      return;
    }

    const totalAmount = calculateTotal();
    if (totalAmount <= 0) {
      toast.error('The total amount of the receipt must be greater than zero.');
      return;
    }

    const receiptData: CreateReceiptDto = {
      customerId: customer.id,
      type: receiptType,
      notes: notes || undefined,
      items: items,
    };

    try {
      await createReceiptMutation.mutateAsync(receiptData);
      toast.success('Receipt created successfully!');
      onClose();
      // Reset form
      setReceiptType('Deposit');
      setNotes('');
      setItems([{ description: '', unitPrice: 0, quantity: 1 }]);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Failed to create receipt.';
      toast.error(errorMessage);
      console.error('Error creating receipt:', error);
    }
  };

  return (
    <Transition appear show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-10" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-black bg-opacity-25" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4 text-center">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Dialog.Panel className="w-full max-w-2xl transform overflow-hidden rounded-2xl bg-white p-6 text-left align-middle shadow-xl transition-all">
                <Dialog.Title as="h3" className="text-lg font-medium leading-6 text-gray-900 flex justify-between items-center">
                  Create New Receipt for {customer.firstName} {customer.lastName}
                  <Button variant="ghost" size="icon" onClick={onClose}>
                    <X className="h-5 w-5" />
                  </Button>
                </Dialog.Title>
                <div className="mt-2">
                  <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                      <Label htmlFor="receiptType">Receipt Type</Label>
                      <Select value={receiptType} onValueChange={(value: 'Deposit' | 'Withdrawal') => setReceiptType(value)}>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Select type" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Deposit">Deposit</SelectItem>
                          <SelectItem value="Withdrawal">Withdrawal</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div>
                      <Label htmlFor="notes">Notes (optional)</Label>
                      <Textarea
                        id="notes"
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                        placeholder="Add notes for this receipt..."
                      />
                    </div>

                    <div className="space-y-3">
                      <h4 className="text-md font-medium flex items-center">
                        Receipt Items
                        <Button type="button" variant="outline" size="sm" onClick={handleAddItem} className="ml-auto">
                          <Plus className="h-4 w-4 mr-2" /> Add Item
                        </Button>
                      </h4>
                      {items.map((item, index) => (
                        <div key={index} className="flex items-end gap-2 border p-3 rounded-md relative">
                          <div className="flex-1 grid grid-cols-3 gap-2">
                            <div>
                              <Label htmlFor={`description-${index}`}>Description</Label>
                              <Input
                                id={`description-${index}`}
                                value={item.description}
                                onChange={(e) => handleItemChange(index, 'description', e.target.value)}
                                placeholder="Item description"
                                required
                              />
                            </div>
                            <div>
                              <Label htmlFor={`unitPrice-${index}`}>Unit Price</Label>
                              <Input
                                id={`unitPrice-${index}`}
                                type="number"
                                step="0.01"
                                min="0"
                                value={item.unitPrice}
                                onChange={(e) => handleItemChange(index, 'unitPrice', e.target.value)}
                                placeholder="0.00"
                                required
                              />
                            </div>
                            <div>
                              <Label htmlFor={`quantity-${index}`}>Quantity</Label>
                              <Input
                                id={`quantity-${index}`}
                                type="number"
                                step="1"
                                min="1"
                                value={item.quantity}
                                onChange={(e) => handleItemChange(index, 'quantity', e.target.value)}
                                placeholder="1"
                                required
                              />
                            </div>
                          </div>
                          {items.length > 1 && (
                            <Button type="button" variant="destructive" size="icon" onClick={() => handleRemoveItem(index)} className="flex-shrink-0">
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      ))}
                    </div>

                    <div className="flex justify-between items-center pt-4 border-t">
                      <span className="text-lg font-semibold">Total:</span>
                      <span className="text-2xl font-bold flex items-center">
                        <DollarSign className="h-6 w-6 mr-1" />
                        {calculateTotal().toFixed(2)}
                      </span>
                    </div>

                    <div className="mt-4 flex justify-end gap-2">
                      <Button type="button" variant="outline" onClick={onClose}>
                        Cancel
                      </Button>
                      <Button type="submit" disabled={createReceiptMutation.isPending}>
                        {createReceiptMutation.isPending ? 'Creating...' : 'Create Receipt'}
                      </Button>
                    </div>
                  </form>
                </div>
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition>
  );
};
