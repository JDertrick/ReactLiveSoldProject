import { useState } from 'react';
import { useGetOrganizations, useCreateOrganization, useUpdateOrganization } from '../../hooks/useOrganizations';
import { CreateOrganizationDto, UpdateOrganizationDto, Organization } from '../../types/organization.types';

const OrganizationsPage = () => {
  const { data: organizations, isLoading } = useGetOrganizations();
  const createOrganization = useCreateOrganization();
  const updateOrganization = useUpdateOrganization();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingOrg, setEditingOrg] = useState<Organization | null>(null);
  const [formData, setFormData] = useState<CreateOrganizationDto | UpdateOrganizationDto>({
    name: '',
    slug: '',
    planType: 'Standard',
    isActive: true,
    logoUrl: '',
  });

  const handleOpenModal = (org?: Organization) => {
    if (org) {
      setEditingOrg(org);
      setFormData({
        name: org.name,
        slug: org.slug,
        planType: org.planType,
        isActive: org.isActive,
        logoUrl: org.logoUrl || '',
      });
    } else {
      setEditingOrg(null);
      setFormData({
        name: '',
        slug: '',
        planType: 'Standard',
        isActive: true,
        logoUrl: '',
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

    try {
      if (editingOrg) {
        await updateOrganization.mutateAsync({
          id: editingOrg.id,
          data: formData as UpdateOrganizationDto,
        });
      } else {
        await createOrganization.mutateAsync(formData as CreateOrganizationDto);
      }
      handleCloseModal();
    } catch (error) {
      console.error('Error saving organization:', error);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value,
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
          <h1 className="text-2xl font-semibold text-gray-900">Organizations</h1>
          <p className="mt-2 text-sm text-gray-700">
            A list of all organizations in the platform including their name, slug, plan, and status.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <button
            type="button"
            onClick={() => handleOpenModal()}
            className="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 sm:w-auto"
          >
            Add Organization
          </button>
        </div>
      </div>

      <div className="mt-8 flex flex-col">
        <div className="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
          <div className="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
            <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
              <table className="min-w-full divide-y divide-gray-300">
                <thead className="bg-gray-50">
                  <tr>
                    <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                      Name
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Slug
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Plan
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Status
                    </th>
                    <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                      <span className="sr-only">Edit</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 bg-white">
                  {organizations && organizations.length > 0 ? (
                    organizations.map((org) => (
                      <tr key={org.id}>
                        <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm sm:pl-6">
                          <div className="flex items-center">
                            {org.logoUrl && (
                              <div className="h-10 w-10 flex-shrink-0">
                                <img className="h-10 w-10 rounded-full" src={org.logoUrl} alt="" />
                              </div>
                            )}
                            <div className={org.logoUrl ? 'ml-4' : ''}>
                              <div className="font-medium text-gray-900">{org.name}</div>
                            </div>
                          </div>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          <code className="text-xs bg-gray-100 px-2 py-1 rounded">/{org.slug}</code>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{org.planType}</td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          <span
                            className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                              org.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                            }`}
                          >
                            {org.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                          <button
                            onClick={() => handleOpenModal(org)}
                            className="text-indigo-600 hover:text-indigo-900"
                          >
                            Edit
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-3 py-4 text-sm text-gray-500 text-center">
                        No organizations found. Create one to get started.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed z-10 inset-0 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={handleCloseModal}></div>

            <div className="inline-block align-bottom bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full sm:p-6">
              <form onSubmit={handleSubmit}>
                <div>
                  <h3 className="text-lg leading-6 font-medium text-gray-900">
                    {editingOrg ? 'Edit Organization' : 'Create Organization'}
                  </h3>
                  <div className="mt-6 space-y-4">
                    <div>
                      <label htmlFor="name" className="block text-sm font-medium text-gray-700">
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
                      <label htmlFor="slug" className="block text-sm font-medium text-gray-700">
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
                        Will be used in portal URL: /portal/{formData.slug || 'slug'}/login
                      </p>
                    </div>

                    <div>
                      <label htmlFor="planType" className="block text-sm font-medium text-gray-700">
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
                      <label htmlFor="logoUrl" className="block text-sm font-medium text-gray-700">
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

                    <div className="flex items-center">
                      <input
                        id="isActive"
                        name="isActive"
                        type="checkbox"
                        checked={formData.isActive}
                        onChange={handleChange}
                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                      />
                      <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                        Active
                      </label>
                    </div>
                  </div>
                </div>

                <div className="mt-5 sm:mt-6 sm:grid sm:grid-cols-2 sm:gap-3 sm:grid-flow-row-dense">
                  <button
                    type="submit"
                    disabled={createOrganization.isPending || updateOrganization.isPending}
                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:col-start-2 sm:text-sm disabled:opacity-50"
                  >
                    {createOrganization.isPending || updateOrganization.isPending ? 'Saving...' : 'Save'}
                  </button>
                  <button
                    type="button"
                    onClick={handleCloseModal}
                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:col-start-1 sm:text-sm"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrganizationsPage;
