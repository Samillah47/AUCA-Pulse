import React from 'react';
import { useAuthStore } from '../../store/authStore';
import { ROLES, VERIFICATION_STATUS } from '../../utils/constants';
import Card from '../common/Card';
import Button from '../common/Button';
import Badge from '../common/Badge';
import { toast } from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService } from '../../services';
import LoadingSpinner from '../common/LoadingSpinner';

const VerificationStatus = () => {
    const { user } = useAuthStore();
    const queryClient = useQueryClient();

    const { data: verificationStatus, isLoading } = useQuery({
        queryKey: ['verificationStatus', user.id],
        queryFn: userService.getVerificationStatus,
        enabled: user?.role === ROLES.LECTURER || user?.role === ROLES.STAFF,
    });

    const requestVerificationMutation = useMutation({
        mutationFn: userService.requestVerification,
        onSuccess: () => {
            toast.success('Verification request sent successfully!');
            queryClient.invalidateQueries(['verificationStatus', user.id]);
        },
        onError: (error) => {
            toast.error(error.message || 'Failed to send verification request.');
        },
    });

    const handleRequestVerification = () => {
        // We can pass additional data if the backend needs it, e.g., from a form.
        requestVerificationMutation.mutate({});
    };

    if (user?.role !== ROLES.LECTURER && user?.role !== ROLES.STAFF) {
        return null;
    }

    if(isLoading) {
        return (
            <Card>
                <Card.Header>
                    <h3 className="text-lg font-semibold">Verification Status</h3>
                </Card.Header>
                <Card.Body>
                    <LoadingSpinner/>
                </Card.Body>
            </Card>
        )
    }

    const renderStatus = () => {
        const status = verificationStatus?.status || 'NOT_SUBMITTED';

        switch (status) {
            case VERIFICATION_STATUS.APPROVED:
                return <Badge color="green">Verified</Badge>;
            case VERIFICATION_STATUS.PENDING:
                return <Badge color="yellow">Pending Verification</Badge>;
            case VERIFICATION_STATUS.REJECTED:
                return (
                    <div>
                        <Badge color="red">Verification Rejected</Badge>
                        <p className="text-sm text-gray-500 mt-2">
                           Reason: {verificationStatus.rejectionReason || 'No reason provided.'}
                        </p>
                         <Button onClick={handleRequestVerification} loading={requestVerificationMutation.isLoading} className="mt-4">
                            Re-submit Request
                        </Button>
                    </div>
                );
            default: // NOT_SUBMITTED
                return (
                    <div>
                        <p className="text-sm text-gray-500 mb-4">
                            You are not verified. Request verification to get access to all features.
                        </p>
                        <Button onClick={handleRequestVerification} loading={requestVerificationMutation.isLoading}>Request Verification</Button>
                    </div>
                );
        }
    }

    return (
        <Card>
            <Card.Header>
                <h3 className="text-lg font-semibold">Verification Status</h3>
            </Card.Header>
            <Card.Body>
                {renderStatus()}
            </Card.Body>
        </Card>
    );
};

export default VerificationStatus;
