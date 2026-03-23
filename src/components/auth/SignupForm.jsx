import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import { AnimatePresence, motion } from 'framer-motion';
import { User, Lock, Mail, Building, Briefcase, Phone, MapPin, Hash } from 'lucide-react';

import authService from '../../services/authService';
import locationService from '../../services/locationService';
import { validationRules } from '../../utils/validators';
import { ROLES } from '../../utils/constants';

import Button from '../common/Button';
import Input from '../common/Input';
import Select from '../common/Select';

const steps = [
  { id: 1, name: 'Account Details' },
  { id: 2, name: 'Personal Information' },
  { id: 3, name: 'Finalize' },
];

const SignupForm = () => {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(1);
  const [locations, setLocations] = useState({ provinces: [], districts: [], sectors: [] });
  const [loadingState, setLoadingState] = useState({
    provinces: false,
    districts: false,
    sectors: false,
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    watch,
    trigger,
    setError,
    setValue,
  } = useForm({
    mode: 'onBlur',
  });

  const selectedProvince = watch('provinceId');
  const selectedDistrict = watch('districtId');
  const selectedRole = watch('roleType');

  // --- Location Data Fetching ---
  useEffect(() => {
    const fetchProvinces = async () => {
      setLoadingState(prev => ({ ...prev, provinces: true }));
      try {
        const provinces = await locationService.getLocationsByType('PROVINCE');
        setLocations(prev => ({ ...prev, provinces }));
      } catch (error) {
        toast.error('Failed to load location data.');
      } finally {
        setLoadingState(prev => ({ ...prev, provinces: false }));
      }
    };
    fetchProvinces();
  }, []);

  useEffect(() => {
    if (!selectedProvince) {
      setLocations(prev => ({ ...prev, districts: [], sectors: [] }));
      return;
    }
    const fetchDistricts = async () => {
      setLoadingState(prev => ({ ...prev, districts: true }));
      setValue('districtId', '');
      setValue('sectorId', '');
      try {
        const districts = await locationService.getChildLocations(selectedProvince);
        setLocations(prev => ({ ...prev, districts, sectors: [] }));
      } catch (error) {
        toast.error('Failed to load districts.');
      } finally {
        setLoadingState(prev => ({ ...prev, districts: false }));
      }
    };
    fetchDistricts();
  }, [selectedProvince, setValue]);

  useEffect(() => {
    if (!selectedDistrict) {
      setLocations(prev => ({ ...prev, sectors: [] }));
      return;
    }
    const fetchSectors = async () => {
      setLoadingState(prev => ({ ...prev, sectors: true }));
      setValue('sectorId', '');
      try {
        const sectors = await locationService.getChildLocations(selectedDistrict);
        setLocations(prev => ({ ...prev, sectors }));
      } catch (error) {
        toast.error('Failed to load sectors.');
      } finally {
        setLoadingState(prev => ({ ...prev, sectors: false }));
      }
    };
    fetchSectors();
  }, [selectedDistrict, setValue]);

  // --- Form Navigation ---
  const handleNext = async () => {
    const fieldsToValidate = {
      1: ['name', 'email', 'password'],
      2: ['roleType', 'identificationNumber', selectedRole === ROLES.LECTURER ? 'courseTaught' : selectedRole === ROLES.STAFF ? 'staffRole' : 'department'],
    };
    const isValid = await trigger(fieldsToValidate[currentStep]);
    if (isValid) {
      setCurrentStep(prev => prev + 1);
    }
  };

  const handlePrev = () => {
    setCurrentStep(prev => prev - 1);
  };

  // --- Form Submission ---
  const onSubmit = async (data) => {
    const locationId = data.sectorId || data.districtId || data.provinceId;
    const finalData = {
      ...data,
      locationId: locationId ? Number(locationId) : null,
    };

    // Remove temporary location fields
    delete finalData.provinceId;
    delete finalData.districtId;
    delete finalData.sectorId;

    // Map role-specific fields to backend field names
    if (data.roleType === ROLES.LECTURER && data.courseTaught) {
      finalData.department = data.courseTaught;
      delete finalData.courseTaught;
    } else if (data.roleType === ROLES.STAFF && data.staffRole) {
      finalData.department = data.staffRole;
      delete finalData.staffRole;
    }

    try {
      const response = await authService.register(finalData);
      toast.success(response.message || 'Registration successful! Please wait for admin approval.');
      navigate('/login');
    } catch (error) {
      const errorMessage = error.message || 'Registration failed. Please try again.';
      toast.error(errorMessage);
      setError('root.serverError', { type: 'manual', message: errorMessage });
    }
  };
  


  return (
    <>
      {/* Progress Bar */}
      <nav aria-label="Progress">
        <ol role="list" className="flex items-center mb-8">
          {steps.map((step, stepIdx) => (
            <li key={step.name} className={`flex-1 ${stepIdx !== steps.length - 1 ? 'pr-8 sm:pr-20' : ''} relative`}>
              {currentStep > step.id ? (
                <>
                  <div className="absolute inset-0 flex items-center" aria-hidden="true">
                    <div className="h-0.5 w-full bg-primary-600" />
                  </div>
                  <span
                    className="relative w-8 h-8 flex items-center justify-center bg-primary-600 rounded-full text-white cursor-pointer"
                    onClick={() => setCurrentStep(step.id)}
                  >
                    &#10003;
                  </span>
                </>
              ) : currentStep === step.id ? (
                <>
                  <div className="absolute inset-0 flex items-center" aria-hidden="true">
                    <div className="h-0.5 w-full bg-gray-200 dark:bg-dark-700" />
                  </div>
                  <span className="relative w-8 h-8 flex items-center justify-center bg-white dark:bg-dark-800 border-2 border-primary-600 rounded-full">
                    <span className="h-2.5 w-2.5 bg-primary-600 rounded-full" />
                  </span>
                </>
              ) : (
                <>
                  <div className="absolute inset-0 flex items-center" aria-hidden="true">
                    <div className="h-0.5 w-full bg-gray-200 dark:bg-dark-700" />
                  </div>
                  <span className="relative w-8 h-8 flex items-center justify-center bg-gray-100 dark:bg-dark-700 rounded-full text-gray-500">
                    {step.id}
                  </span>
                </>
              )}
            </li>
          ))}
        </ol>
      </nav>

      {/* Form Content */}
      <form onSubmit={(e) => e.preventDefault()} className="space-y-6">
        <AnimatePresence mode="wait">
          {currentStep === 1 && (
            <motion.div key={1} initial={{ x: 300, opacity: 0 }} animate={{ x: 0, opacity: 1 }} exit={{ x: -300, opacity: 0 }} className="space-y-6">
              <h2 className="text-lg font-medium">Step 1: Account Details</h2>
              <Input label="Full Name" id="name" leftIcon={<User />} error={errors.name?.message} {...register('name', validationRules.name)} />
              <Input label="Email" id="email" type="email" leftIcon={<Mail />} error={errors.email?.message} {...register('email', validationRules.email)} />
              <Input label="Password" id="password" type="password" leftIcon={<Lock />} error={errors.password?.message} {...register('password', validationRules.password)} />
            </motion.div>
          )}

          {currentStep === 2 && (
            <motion.div key={2} initial={{ x: 300, opacity: 0 }} animate={{ x: 0, opacity: 1 }} exit={{ x: -300, opacity: 0 }} className="space-y-6">
              <h2 className="text-lg font-medium">Step 2: Personal Information</h2>
              <Select label="Role" id="roleType" error={errors.roleType?.message} {...register('roleType', { required: 'Role is required' })}>
                <option value="">Select a role</option>
                <option value={ROLES.STUDENT}>Student</option>
                <option value={ROLES.LECTURER}>Lecturer</option>
                <option value={ROLES.STAFF}>Staff</option>
              </Select>
              <Input label="Identification Number" id="identificationNumber" leftIcon={<Hash />} error={errors.identificationNumber?.message} {...register('identificationNumber', validationRules.identificationNumber)} />
              
              {/* Conditional field based on role */}
              {selectedRole === ROLES.LECTURER && (
                <Input label="Course Taught" id="courseTaught" leftIcon={<Briefcase />} error={errors.courseTaught?.message} {...register('courseTaught', { required: 'Course taught is required for lecturers' })} />
              )}
              
              {selectedRole === ROLES.STAFF && (
                <Input label="Role" id="staffRole" leftIcon={<Briefcase />} error={errors.staffRole?.message} {...register('staffRole', { required: 'Role is required for staff' })} />
              )}
              
              {(selectedRole === ROLES.STUDENT || !selectedRole) && (
                <Input label="Department" id="department" leftIcon={<Briefcase />} error={errors.department?.message} {...register('department')} />
              )}
            </motion.div>
          )}

          {currentStep === 3 && (
            <motion.div key={3} initial={{ x: 300, opacity: 0 }} animate={{ x: 0, opacity: 1 }} exit={{ x: -300, opacity: 0 }} className="space-y-6">
              <h2 className="text-lg font-medium">Step 3: Contact & Location</h2>
              <Input label="Phone Number (Optional)" id="phoneNumber" type="tel" leftIcon={<Phone />} error={errors.phoneNumber?.message} {...register('phoneNumber')} />
              <Select label="Province" id="provinceId" error={errors.provinceId?.message} {...register('provinceId')} disabled={loadingState.provinces}>
                <option value="">Select Province</option>
                {locations.provinces.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
              </Select>
              <Select label="District" id="districtId" error={errors.districtId?.message} {...register('districtId')} disabled={!selectedProvince || loadingState.districts}>
                <option value="">Select District</option>
                {locations.districts.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
              </Select>
              <Select label="Sector" id="sectorId" error={errors.sectorId?.message} {...register('sectorId')} disabled={!selectedDistrict || loadingState.sectors}>
                <option value="">Select Sector</option>
                {locations.sectors.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </Select>
              {errors.root?.serverError && <p className="text-sm text-red-500">{errors.root.serverError.message}</p>}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Navigation Buttons */}
        <div className="flex justify-between pt-4">
          {currentStep > 1 ? (
            <Button type="button" variant="outline" onClick={handlePrev}>
              Previous
            </Button>
          ) : <div />}
          {currentStep < steps.length ? (
            <Button type="button" variant="primary" onClick={handleNext}>
              Next
            </Button>
          ) : (
            <Button
              type="button"
              variant="success"
              loading={isSubmitting}
              onClick={handleSubmit(data => {
                const trimmedData = { ...data, email: data.email.trim() };
                onSubmit(trimmedData);
              })}
            >
              Complete Registration
            </Button>
          )}
        </div>
      </form>
    </>
  );
};

export default SignupForm;
