import React from 'react';
import Button from '../components/common/Button';
import Card from '../components/common/Card';
import Input from '../components/common/Input';
import Badge from '../components/common/Badge';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { Mail, User, Lock } from 'lucide-react';

const ComponentTest = () => {
  return (
    <div className="p-8 space-y-8">
      <h1 className="text-3xl font-bold">Component Test Page</h1>

      {/* Buttons */}
      <section>
        <h2 className="text-xl font-semibold mb-4">Buttons</h2>
        <div className="flex flex-wrap gap-4 items-center">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="success">Success</Button>
          <Button variant="danger">Danger</Button>
          <Button variant="warning">Warning</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="primary" loading>Loading</Button>
        </div>
      </section>

      {/* Cards */}
      <section>
        <h2 className="text-xl font-semibold mb-4">Cards</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <Card>
                <Card.Header>Card Header</Card.Header>
                <Card.Body>
                    <p>This is the body of the card. It can contain any content.</p>
                </Card.Body>
                <Card.Footer>Card Footer</Card.Footer>
            </Card>
             <Card>
                <Card.Body>
                    <h3 className="font-bold text-lg mb-2">Card without Header/Footer</h3>
                    <p>This is the body of the card. It can contain any content.</p>
                </Card.Body>
            </Card>
        </div>
      </section>

      {/* Inputs */}
      <section>
        <h2 className="text-xl font-semibold mb-4">Inputs</h2>
        <div className="space-y-4 max-w-sm">
            <Input label="Email" id="test-email" placeholder="your.email@example.com" leftIcon={<Mail />} />
            <Input label="Full Name" id="test-name" placeholder="John Doe" leftIcon={<User />} />
            <Input label="Password" id="test-password" type="password" leftIcon={<Lock />} />
            <Input label="Input with Error" id="test-error" error="This field is required." />
        </div>
      </section>

        {/* Badges */}
      <section>
        <h2 className="text-xl font-semibold mb-4">Badges</h2>
        <div className="flex flex-wrap gap-4 items-center">
            <Badge variant="primary">Primary</Badge>
            <Badge variant="secondary">Secondary</Badge>
            <Badge variant="success">Success</Badge>
            <Badge variant="danger">Danger</Badge>
            <Badge variant="warning">Warning</Badge>
            <Badge status="AVAILABLE">Available</Badge>
            <Badge status="OCCUPIED">Occupied</Badge>
        </div>
      </section>

      {/* Loading Spinners */}
      <section>
        <h2 className="text-xl font-semibold mb-4">Loading Spinners</h2>
        <div className="flex items-center gap-6">
          <LoadingSpinner size="sm" />
          <LoadingSpinner size="md" />
          <LoadingSpinner size="lg" />
          <LoadingSpinner size="xl" />
        </div>
      </section>

    </div>
  );
};

export default ComponentTest;
