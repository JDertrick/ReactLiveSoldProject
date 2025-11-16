import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useLocations } from "../../hooks/useLocations";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Textarea } from "../../components/ui/textarea";
import { toast } from "sonner";
import { Location } from "../../types/location.types";

const LocationFormPage = () => {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();
  const { locations, createLocation, updateLocation, isLoading } = useLocations();

  const [formData, setFormData] = useState<Partial<Location>>({
    name: "",
    description: "",
  });

  useEffect(() => {
    if (id && locations) {
      const locationToEdit = locations.find((loc) => loc.id === id);
      if (locationToEdit) {
        setFormData({
          name: locationToEdit.name,
          description: locationToEdit.description,
        });
      }
    }
  }, [id, locations]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateLocation({ ...formData, id } as Location);
        toast.success("Location updated successfully!");
      } else {
        await createLocation(formData as Location);
        toast.success("Location created successfully!");
      }
      navigate("/app/locations");
    } catch (err) {
      toast.error("Failed to save location.");
      console.error("Error saving location:", err);
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-6">
        {id ? "Edit Location" : "Create New Location"}
      </h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700">
            Name
          </label>
          <Input
            type="text"
            name="name"
            id="name"
            required
            value={formData.name || ""}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="description" className="block text-sm font-medium text-gray-700">
            Description
          </label>
          <Textarea
            name="description"
            id="description"
            value={formData.description || ""}
            onChange={handleChange}
          />
        </div>
        <div className="flex gap-2">
          <Button type="submit">{id ? "Update Location" : "Create Location"}</Button>
          <Button type="button" variant="outline" onClick={() => navigate("/app/locations")}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
};

export default LocationFormPage;
