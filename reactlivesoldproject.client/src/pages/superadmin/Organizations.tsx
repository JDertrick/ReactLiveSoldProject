import { useState } from "react";
import { Dialog, DialogBackdrop, DialogPanel, DialogTitle } from "@headlessui/react";
import {
  useGetOrganizations,
  useCreateOrganization,
  useUpdateOrganization,
} from "../../hooks/useOrganizations";
import {
  CreateOrganizationDto,
  Organization,
} from "../../types/organization.types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";
import { Button } from "../../components/ui/button";
import { Badge } from "../../components/ui/badge";
import { Link, Outlet } from "react-router-dom";

const OrganizationsPage = () => {
  const { data: organizations, isLoading } = useGetOrganizations();
  const createOrganization = useCreateOrganization();
  const updateOrganization = useUpdateOrganization();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingOrg, setEditingOrg] = useState<Organization | null>(null);
  const [formData, setFormData] = useState<CreateOrganizationDto>({
    name: "",
    slug: "",
    planType: "Standard",
    logoUrl: "",
    primaryContactEmail: "",
  });

  const handleOpenModal = (org?: Organization) => {
    if (org) {
      setEditingOrg(org);
      setFormData({
        name: org.name,
        slug: org.slug,
        planType: org.planType,
        logoUrl: org.logoUrl || "",
        primaryContactEmail: org.primaryContactEmail || "",
      });
    } else {
      setEditingOrg(null);
      setFormData({
        name: "",
        slug: "",
        planType: "Standard",
        logoUrl: "",
        primaryContactEmail: "",
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingOrg(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Prepare data: convert empty strings to undefined for optional fields
    const submitData: CreateOrganizationDto = {
      name: formData.name,
      slug: formData.slug?.trim() || undefined,
      logoUrl: formData.logoUrl?.trim() || undefined,
      primaryContactEmail: formData.primaryContactEmail,
      planType: formData.planType,
    };

    console.log("Submitting formData:", submitData);

    try {
      if (editingOrg) {
        await updateOrganization.mutateAsync({
          id: editingOrg.id,
          data: submitData,
        });
      } else {
        await createOrganization.mutateAsync(submitData);
      }
      handleCloseModal();
    } catch (error: any) {
      console.error("Error saving organization:", error);
      console.error("Response data:", error.response?.data);
      console.error("Validation errors:", error.response?.data?.errors);
      console.error("Response status:", error.response?.status);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-semibold text-gray-900">
            Organizations
          </h1>
          <p className="mt-2 text-sm text-gray-700">
            A list of all organizations in the platform including their name,
            slug, plan, and status.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <Button onClick={() => handleOpenModal()}>
            Add Organization
          </Button>
        </div>
      </div>

      {/* Desktop Table View */}
      <div className="mt-8 hidden md:block rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Plan</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {organizations && organizations.length > 0 ? (
              organizations.map((org) => (
                <TableRow key={org.id}>
                  <TableCell>
                    <div className="flex items-center">
                      {org.logoUrl && (
                        <div className="h-10 w-10 flex-shrink-0">
                          <img
                            className="h-10 w-10 rounded-full"
                            src={org.logoUrl}
                            alt=""
                          />
                        </div>
                      )}
                      <div className={org.logoUrl ? "ml-4" : ""}>
                        <div className="font-medium text-gray-900">
                          {org.name}
                        </div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <code className="text-xs bg-gray-100 px-2 py-1 rounded">
                      /{org.slug}
                    </code>
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary">{org.planType}</Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={org.isActive ? "default" : "destructive"}>
                      {org.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleOpenModal(org)}
                      >
                        Edit
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        asChild
                      >
                        <Link to={`/superadmin/organizations/${org.id}/users`}>
                          Users
                        </Link>
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={5}
                  className="h-24 text-center"
                >
                  No organizations found. Create one to get started.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {/* Mobile Card View */}
      <div className="mt-8 md:hidden space-y-4">
        {organizations && organizations.length > 0 ? (
          organizations.map((org) => (
            <div
              key={org.id}
              className="bg-white border rounded-lg p-4 shadow-sm space-y-3"
            >
              <div className="flex justify-between items-start">
                <div className="flex items-center gap-3">
                  {org.logoUrl && (
                    <img
                      className="h-12 w-12 rounded-full"
                      src={org.logoUrl}
                      alt=""
                    />
                  )}
                  <div>
                    <div className="font-medium text-lg">{org.name}</div>
                    <code className="text-xs bg-gray-100 px-2 py-1 rounded">
                      /{org.slug}
                    </code>
                  </div>
                </div>
                <Badge variant={org.isActive ? "default" : "destructive"}>
                  {org.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>

              <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
                <div>
                  <div className="text-muted-foreground">Plan</div>
                  <Badge variant="secondary">{org.planType}</Badge>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-2 pt-2 border-t">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleOpenModal(org)}
                >
                  Edit
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  asChild
                >
                  <Link to={`/superadmin/organizations/${org.id}/users`}>
                    Users
                  </Link>
                </Button>
              </div>
            </div>
          ))
        ) : (
          <div className="bg-white border rounded-lg p-8 text-center">
            <p className="text-muted-foreground">
              No organizations found. Create one to get started.
            </p>
          </div>
        )}
      </div>

      <Outlet />

      {/* Modal with Headless UI Dialog */}
      <Dialog open={isModalOpen} onClose={handleCloseModal} className="relative z-10">
        <DialogBackdrop
          transition
          className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
        />

        <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <DialogPanel
              transition
              className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-lg data-closed:sm:translate-y-0 data-closed:sm:scale-95"
            >
              <form onSubmit={handleSubmit}>
                <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                  <DialogTitle as="h3" className="text-lg leading-6 font-medium text-gray-900">
                    {editingOrg ? "Edit Organization" : "Create Organization"}
                  </DialogTitle>
                  <div className="mt-6 space-y-4">
                    <div>
                      <label
                        htmlFor="name"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Name
                      </label>
                      <input
                        type="text"
                        name="name"
                        id="name"
                        required
                        value={formData.name}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>

                    <div>
                      <label
                        htmlFor="slug"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Slug (URL identifier)
                      </label>
                      <input
                        type="text"
                        name="slug"
                        id="slug"
                        required
                        value={formData.slug}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        placeholder="e.g., my-store"
                      />
                      <p className="mt-1 text-xs text-gray-500">
                        Will be used in portal URL: /portal/
                        {formData.slug || "slug"}/login
                      </p>
                    </div>

                    <div>
                      <label
                        htmlFor="planType"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Plan Type
                      </label>
                      <select
                        name="planType"
                        id="planType"
                        value={formData.planType}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      >
                        <option value="Free">Free</option>
                        <option value="Standard">Standard</option>
                        <option value="Premium">Premium</option>
                      </select>
                    </div>

                    <div>
                      <label
                        htmlFor="primaryContactEmail"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Primary Contact Email
                      </label>
                      <input
                        type="email"
                        name="primaryContactEmail"
                        id="primaryContactEmail"
                        required
                        value={formData.primaryContactEmail}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        placeholder="contact@organization.com"
                      />
                    </div>

                    <div>
                      <label
                        htmlFor="logoUrl"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Logo URL (optional)
                      </label>
                      <input
                        type="url"
                        name="logoUrl"
                        id="logoUrl"
                        value={formData.logoUrl}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        placeholder="https://example.com/logo.png"
                      />
                    </div>

                    {editingOrg && (
                      <div className="rounded-md bg-blue-50 p-4">
                        <div className="flex">
                          <div className="flex-shrink-0">
                            <svg
                              className="h-5 w-5 text-blue-400"
                              xmlns="http://www.w3.org/2000/svg"
                              viewBox="0 0 20 20"
                              fill="currentColor"
                            >
                              <path
                                fillRule="evenodd"
                                d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                                clipRule="evenodd"
                              />
                            </svg>
                          </div>
                          <div className="ml-3 flex-1">
                            <p className="text-sm text-blue-700">
                              Current status:{" "}
                              <span
                                className={`font-semibold ${
                                  editingOrg.isActive
                                    ? "text-green-700"
                                    : "text-red-700"
                                }`}
                              >
                                {editingOrg.isActive ? "Active" : "Inactive"}
                              </span>
                              <br />
                              <span className="text-xs">
                                To change the status, please contact support.
                              </span>
                            </p>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

                <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                  <button
                    type="submit"
                    disabled={
                      createOrganization.isPending ||
                      updateOrganization.isPending
                    }
                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
                  >
                    {createOrganization.isPending ||
                    updateOrganization.isPending
                      ? "Saving..."
                      : "Save"}
                  </button>
                  <button
                    type="button"
                    onClick={handleCloseModal}
                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </DialogPanel>
          </div>
        </div>
      </Dialog>
    </div>
  );
};

export default OrganizationsPage;
