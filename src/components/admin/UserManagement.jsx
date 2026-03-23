import React, { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { Edit, Trash2, UserPlus } from 'lucide-react';
import { format } from 'date-fns';
import { useForm } from 'react-hook-form';

import { adminService } from '../../services';
import useDebounce from '../../hooks/useDebounce';
import { Table, Button, LoadingSpinner, EmptyState, Modal, Badge, Avatar, Input, Select, Pagination } from '../common';
import { ROLES } from '../../utils/constants';

const UserManagement = () => {
    const queryClient = useQueryClient();
    const [filters, setFilters] = useState({ page: 0, size: 10, search: '', role: '' });
    const debouncedSearch = useDebounce(filters.search, 500);
    const [action, setAction] = useState(null); // { type: 'edit' | 'delete', user: {...} }

    const queryFilters = { ...filters, search: debouncedSearch };
    
    const { data: usersPage, isLoading, error } = useQuery({
        queryKey: ['users', queryFilters],
        queryFn: () => adminService.getUsers(queryFilters),
        keepPreviousData: true
    });

    const updateUserMutation = useMutation({
        mutationFn: ({ userId, userData }) => adminService.updateUser(userId, userData),
        onSuccess: () => {
            toast.success('User updated successfully!');
            queryClient.invalidateQueries('users');
            setAction(null);
        },
        onError: (err) => toast.error(err.message || 'Failed to update user.'),
    });

    const deleteUserMutation = useMutation({
        mutationFn: adminService.deleteUser,
        onSuccess: () => {
            toast.success('User deleted successfully!');
            queryClient.invalidateQueries('users');
            setAction(null);
        },
        onError: (err) => toast.error(err.message || 'Failed to delete user.'),
    });

    const handleFilterChange = (e) => {
        setFilters(prev => ({ ...prev, [e.target.name]: e.target.value, page: 0 }));
    };
    
        const handlePageChange = (newPage) => {
    
            setFilters(prev => ({...prev, page: newPage - 1})); // Adjust to 0-based index for backend
    
        };
    
    
    
        const columns = useMemo(() => [
    
            {
    
                key: 'name',
    
                label: 'Name',
    
                render: (_, row) => (
    
                    <div className="flex items-center gap-3">
    
                        <Avatar name={row.name} src={row.profilePicture} size="sm" />
    
                        <div>
    
                            <p className="font-medium">{row.name}</p>
    
                            <p className="text-xs text-gray-500">{row.email}</p>
    
                        </div>
    
                    </div>
    
                )
    
            },
    
            { key: 'role', label: 'Role', render: (role) => <Badge>{role}</Badge> },
    
            { key: 'department', label: 'Department' },
    
            {
    
                key: 'createdAt',
    
                label: 'Date Registered',
    
                render: (date) => format(new Date(date), 'MMM d, yyyy')
    
            },
    
            {
    
                key: 'actions',
    
                label: 'Actions',
    
                render: (_, row) => (
    
                     <div className="flex gap-2">
    
                        <Button size="sm" variant="outline" onClick={() => setAction({ type: 'edit', user: row })}>
    
                            <Edit className="w-4 h-4" />
    
                        </Button>
    
                        <Button size="sm" variant="danger" onClick={() => setAction({ type: 'delete', user: row })}>
    
                            <Trash2 className="w-4 h-4" />
    
                        </Button>
    
                    </div>
    
                )
    
            }
    
        ], []);
    
        
    
        return (
    
            <div className="space-y-6">
    
                <div className="flex justify-between items-center">
    
                    <h1 className="text-3xl font-bold tracking-tight">User Management</h1>
    
                     <Button>
    
                        <UserPlus className="mr-2 h-4 w-4" /> Add User
    
                    </Button>
    
                </div>
    
    
    
                {/* Filters */}
    
                <div className="flex gap-4">
    
                    <Input name="search" placeholder="Search by name or email..." value={filters.search} onChange={handleFilterChange} className="flex-1" />
    
                    <Select name="role" value={filters.role} onChange={handleFilterChange}>
    
                        <option value="">All Roles</option>
    
                        {Object.values(ROLES).map(role => <option key={role} value={role}>{role}</option>)}
    
                    </Select>
    
                </div>
    
    
    
                {isLoading && <LoadingSpinner />}
    
                {error && <EmptyState title="Error" description={error.message} />}
    
                {!isLoading && !error && (
    
                    <>
    
                    <Table
    
                        columns={columns}
    
                        data={usersPage?.content || []}
    
                        emptyMessage="No users found."
    
                    />
    
                    <Pagination 
    
                        currentPage={usersPage?.number + 1} // Display 1-based page number
    
                        totalPages={usersPage?.totalPages}
    
                        onPageChange={handlePageChange}
    
                    />
    
                    </>
    
                )}

            {/* Action Modals */}
             {action?.type === 'edit' && (
                <EditUserModal
                    isOpen={true}
                    onClose={() => setAction(null)}
                    user={action.user}
                    onSubmit={updateUserMutation.mutate}
                    isLoading={updateUserMutation.isLoading}
                />
            )}
             {action?.type === 'delete' && (
                <Modal isOpen={true} onClose={() => setAction(null)} title="Delete User">
                    <p>Are you sure you want to delete the user <span className="font-semibold">{action.user.name}</span>? This action cannot be undone.</p>
                    <Modal.Footer
                        onCancel={() => setAction(null)}
                        onConfirm={() => deleteUserMutation.mutate(action.user.id)}
                        confirmText="Delete"
                        loading={deleteUserMutation.isLoading}
                    />
                </Modal>
            )}
        </div>
    );
};

const EditUserModal = ({ isOpen, onClose, user, onSubmit, isLoading }) => {
    const { register, handleSubmit } = useForm({ defaultValues: user });
    
    const handleFormSubmit = (data) => {
        onSubmit({ userId: user.id, userData: data });
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose} title={`Edit ${user.name}`}>
            <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
                <Input label="Full Name" id="name" {...register('name')} />
                <Input label="Email" id="email" type="email" {...register('email')} disabled />
                <Select label="Role" id="role" {...register('role')}>
                     {Object.values(ROLES).map(role => <option key={role} value={role}>{role}</option>)}
                </Select>
                 <Input label="Department" id="department" {...register('department')} />
                 <Modal.Footer
                    onCancel={onClose}
                    onConfirm={handleSubmit(handleFormSubmit)}
                    confirmText="Save Changes"
                    loading={isLoading}
                />
            </form>
        </Modal>
    );
}

export default UserManagement;
