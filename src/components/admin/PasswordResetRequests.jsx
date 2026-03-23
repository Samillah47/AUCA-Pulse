import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { format } from 'date-fns';
import { CheckCircle, XCircle } from 'lucide-react';

import { adminService } from '../../services';
import { useAuthStore } from '../../store';
import { Table, Button, LoadingSpinner, EmptyState, Avatar, Modal, Input } from '../common';

const PasswordResetRequests = () => {
    const queryClient = useQueryClient();
    const { user } = useAuthStore();
    const adminId = user?.userId || user?.id;
    const [rejectModalOpen, setRejectModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState(null);
    const [rejectionReason, setRejectionReason] = useState('');

    const { data: requests, isLoading, error } = useQuery({
        queryKey: ['passwordResetRequests'],
        queryFn: () => adminService.getPasswordResetRequests(),
    });

    const approveMutation = useMutation({
        mutationFn: adminService.approvePasswordResetRequest,
        onSuccess: () => {
            toast.success('Password reset request approved. User has been notified.');
            queryClient.invalidateQueries({ queryKey: ['passwordResetRequests'] });
        },
        onError: (err) => {
            toast.error(err.message || 'Failed to approve request.');
        },
    });

    const rejectMutation = useMutation({
        mutationFn: ({ requestId, adminId, reason }) => 
            adminService.rejectPasswordResetRequest(requestId, adminId, reason),
        onSuccess: () => {
            toast.success('Password reset request rejected. User has been notified.');
            queryClient.invalidateQueries({ queryKey: ['passwordResetRequests'] });
            setRejectModalOpen(false);
            setSelectedRequest(null);
            setRejectionReason('');
        },
        onError: (err) => {
            toast.error(err.message || 'Failed to reject request.');
        },
    });

    const handleApprove = (requestId) => {
        if (!adminId) {
            toast.error('Could not identify admin. Please log in again.');
            return;
        }
        approveMutation.mutate({ requestId, adminId });
    };

    const handleRejectClick = (request) => {
        setSelectedRequest(request);
        setRejectModalOpen(true);
    };

    const handleRejectConfirm = () => {
        if (!rejectionReason.trim()) {
            toast.error('Please provide a reason for rejection.');
            return;
        }
        if (!adminId) {
            toast.error('Could not identify admin. Please log in again.');
            return;
        }
        rejectMutation.mutate({ 
            requestId: selectedRequest.id, 
            adminId, 
            reason: rejectionReason 
        });
    };

    const columns = [
        {
            key: 'user',
            label: 'User',
            render: (_, row) => (
                <div className="flex items-center gap-3">
                    <Avatar name={row.user.name} size="sm" />
                    <div>
                        <p className="font-medium">{row.user.name}</p>
                        <p className="text-xs text-gray-500">{row.user.email}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'createdAt',
            label: 'Request Date',
            render: (date) => format(new Date(date), 'MMM d, yyyy, h:mm a'),
        },
        {
            key: 'actions',
            label: 'Actions',
            render: (_, row) => (
                <div className="flex gap-2">
                    <Button 
                        size="sm" 
                        variant="success" 
                        onClick={() => handleApprove(row.id)}
                        loading={approveMutation.isLoading && approveMutation.variables?.requestId === row.id}
                    >
                        <CheckCircle className="w-4 h-4 mr-2" />
                        Approve
                    </Button>
                    <Button 
                        size="sm" 
                        variant="danger" 
                        onClick={() => handleRejectClick(row)}
                        loading={rejectMutation.isLoading && selectedRequest?.id === row.id}
                    >
                        <XCircle className="w-4 h-4 mr-2" />
                        Reject
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <h1 className="text-3xl font-bold tracking-tight">Password Reset Requests</h1>
            
            {isLoading && <LoadingSpinner />}
            {error && <EmptyState title="Error" description={error.message} />}
            {!isLoading && !error && (
                <Table
                    columns={columns}
                    data={requests || []}
                    emptyMessage="No pending password reset requests."
                />
            )}

            <Modal 
                isOpen={rejectModalOpen} 
                onClose={() => {
                    setRejectModalOpen(false);
                    setSelectedRequest(null);
                    setRejectionReason('');
                }}
                title="Reject Password Reset Request"
            >
                <div className="space-y-4">
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                        You are about to reject the password reset request from <strong>{selectedRequest?.user?.name}</strong>.
                    </p>
                    <Input
                        label="Reason for Rejection"
                        placeholder="e.g., Suspicious activity detected, Invalid request, etc."
                        value={rejectionReason}
                        onChange={(e) => setRejectionReason(e.target.value)}
                        required
                    />
                    <Modal.Footer
                        onCancel={() => {
                            setRejectModalOpen(false);
                            setSelectedRequest(null);
                            setRejectionReason('');
                        }}
                        onConfirm={handleRejectConfirm}
                        confirmText="Reject Request"
                        confirmVariant="danger"
                        loading={rejectMutation.isLoading}
                    />
                </div>
            </Modal>
        </div>
    );
};

export default PasswordResetRequests;