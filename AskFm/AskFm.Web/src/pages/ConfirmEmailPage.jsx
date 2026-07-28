import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { confirmEmail } from '../api/auth';
import { parseApiError } from '../api/client';
import { CheckCircle, AlertCircle } from 'lucide-react';
import { Spinner } from '../components/Spinner';

export const ConfirmEmailPage = () => {
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email');
  const token = searchParams.get('token');

  const [loading, setLoading] = useState(true);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!email || !token) {
      setLoading(false);
      setError('Invalid confirmation link.');
      return;
    }

    const confirm = async () => {
      try {
        await confirmEmail({ email, token });
        setSuccess(true);
      } catch (err) {
        setError(parseApiError(err));
      } finally {
        setLoading(false);
      }
    };

    confirm();
  }, [email, token]);

  return (
    <div style={{ maxWidth: '400px', margin: '80px auto', padding: '0 16px', textAlign: 'center' }}>
      <div className="card">
        {loading ? (
          <Spinner />
        ) : success ? (
          <div>
            <CheckCircle style={{ width: '48px', height: '48px', color: '#16a34a', margin: '0 auto 12px' }} />
            <h2 style={{ fontSize: '20px', fontWeight: '800', marginBottom: '8px' }}>Email Confirmed!</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '14px', marginBottom: '20px' }}>
              Your email has been verified successfully.
            </p>
            <Link to="/login" className="btn btn-primary">
              Log In Now
            </Link>
          </div>
        ) : (
          <div>
            <AlertCircle style={{ width: '48px', height: '48px', color: '#dc2626', margin: '0 auto 12px' }} />
            <h2 style={{ fontSize: '20px', fontWeight: '800', marginBottom: '8px' }}>Verification Failed</h2>
            <p style={{ color: '#dc2626', fontSize: '14px', marginBottom: '20px' }}>{error}</p>
            <Link to="/login" className="btn btn-secondary">
              Back to Login
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};
