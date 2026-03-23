import React, { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { Check, X } from 'lucide-react';
import { format } from 'date-fns';

import { adminService } from '../../services';
import { Table, Button, LoadingSpinner, EmptyState, Modal, Avatar, Input } from '../common';
import { VERIFICATION_STATUS } from '../../utils/constants';
import { useAuthStore } from '../../store/authStore';
import useDebounce from '../../hooks/useDebounce';

const VerificationRequests = () => {
    const queryClient = useQueryClient();
    const user = useAuthStore((state) => state.user);
    const adminId = user?.userId;
    
    const [filters, setFilters] = useState({
        status: VERIFICATION_STATUS.PENDING,
        search: '',
    });
    const debouncedSearch = useDebounce(filters.search, 500);
    const [action, setAction] = useState(null);

    const queryFilters = { ...filters, search: debouncedSearch };

    const { data: requests, isLoading, error } = useQuery({
        queryKey: ['verificationRequests', queryFilters],
        queryFn: () => adminService.getVerificationRequests(queryFilters),
        keepPreviousData: true,
    });

    const approveMutation = useMutation({
        mutationFn: ({ requestId, adminId }) => adminService.approveUser(requestId, adminId),
        onSuccess: () => {
            toast.success('User approved successfully!');
            queryClient.invalidateQueries({ queryKey: ['verificationRequests'] });
            setAction(null);
        },
        onError: (err) => toast.error(err.message || 'Failed to approve user.'),
    });

    const rejectMutation = useMutation({
        mutationFn: ({ requestId, reason }) => adminService.rejectUser(requestId, reason),
        onSuccess: () => {
            toast.success('User rejected successfully!');
            queryClient.invalidateQueries({ queryKey: ['verificationRequests'] });
            setAction(null);
        },
        onError: (err) => toast.error(err.message || 'Failed to reject user.'),
    });
    
    const handleFilterChange = (key, value) => {
        setFilters(prev => ({ ...prev, [key]: value }));
    };

    const columns = useMemo(() => [
        {
            key: 'user',
            label: 'User',
            render: (_, row) => (
                <div className="flex items-center gap-3">
                    <Avatar name={row.userName} size="sm" />
                    <div>
                        <p className="font-medium">{row.userName}</p>
                        <p className="text-xs text-gray-500">{row.email}</p>
                    </div>
                </div>
            )
        },
        { key: 'requestType', label: 'Role' },
        { key: 'submittedId', label: 'ID Number' },
        {
            key: 'submittedAt',
            label: 'Request Date',
            render: (date) => format(new Date(date), 'MMM d, yyyy')
        },
        {
            key: 'actions',
            label: 'Actions',
            render: (_, row) => (
                filters.status === VERIFICATION_STATUS.PENDING && (
                    <div className="flex gap-2">
                        <Button size="sm" variant="success" onClick={() => setAction({ type: 'approve', request: row })}>
                            <Check className="w-4 h-4" /> Approve
                        </Button>
                        <Button size="sm" variant="danger" onClick={() => setAction({ type: 'reject', request: row })}>
                            <X className="w-4 h-4" /> Reject
                        </Button>
                    </div>
                )
            )
        }
    ], [filters.status]);
    
    return (
        <div className="space-y-6">
            <div className="flex flex-col md:flex-row md:justify-between md:items-center gap-4">
                <h1 className="text-3xl font-bold tracking-tight">Verification Requests</h1>
                <div className="flex items-center gap-4">
                    <Input
                        name="search"
                        placeholder="Search by name, email, ID..."
                        value={filters.search}
                        onChange={(e) => handleFilterChange('search', e.target.value)}
                        className="w-full md:w-64"
                    />
                    <div className="flex gap-2 p-1 bg-gray-100 dark:bg-dark-800 rounded-lg">
                        {Object.values(VERIFICATION_STATUS).map(status => (
                            <Button
                                key={status}
                                variant={filters.status === status ? 'primary' : 'ghost'}
                                size="sm"
                                onClick={() => handleFilterChange('status', status)}
                            >
                                {status}
                            </Button>
                        ))}
                    </div>
                </div>
            </div>

            {isLoading && <LoadingSpinner />}
            {error && <EmptyState title="Error" description={error.message} />}
            {!isLoading && !error && (
                <Table
                    columns={columns}
                    data={requests || []}
                    emptyMessage="No verification requests found."
                />
            )}

            {action?.type === 'approve' && (
                <Modal isOpen={true} onClose={() => setAction(null)} title="Approve User">
                    <p>Are you sure you want to approve the user <span className="font-semibold">{action.request.userName}</span> as a <span className="font-semibold">{action.request.requestType}</span>?</p>
                    <Modal.Footer
                        onCancel={() => setAction(null)}
                        onConfirm={() => approveMutation.mutate({ requestId: action.request.requestId, adminId })}
                        confirmText="Approve"
                        loading={approveMutation.isLoading}
                    />
                </Modal>
            )}

            {action?.type === 'reject' && (
                <RejectModal 
                    isOpen={true} 
                    onClose={() => setAction(null)} 
                    onSubmit={(reason) => rejectMutation.mutate({ requestId: action.request.requestId, reason })}
                    isLoading={rejectMutation.isLoading}
                    user={{ name: action.request.userName }}
                />
            )}
        </div>
    );
};

const RejectModal = ({ isOpen, onClose, onSubmit, isLoading, user }) => {
    const [reason, setReason] = useState('');

    const handleSubmit = () => {
        if (!reason) {
            toast.error('Please provide a reason for rejection.');
            return;
        }
        onSubmit(reason);
    }
    
    return (
        <Modal isOpen={isOpen} onClose={onClose} title={`Reject ${user.name}`}>
            <div className="space-y-4">
                <p>Please provide a reason for rejecting this verification request. This will be sent to the user.</p>
                <textarea
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    className="w-full p-2 border rounded-md bg-gray-50 dark:bg-dark-700 border-gray-300 dark:border-dark-600"
                    rows="4"
                    placeholder="e.g., Identification number does not match records."
                />
            </div>
             <Modal.Footer
                onCancel={onClose}
                onConfirm={handleSubmit}
                confirmText="Reject"
                loading={isLoading}
            />
        </Modal>
    );
}

export default VerificationRequests;
