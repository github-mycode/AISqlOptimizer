import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Container,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
  Alert,
  Divider,
  CircularProgress,
} from '@mui/material';
import { Controller, useForm } from 'react-hook-form';
import { useMutation } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';
import type { DatabaseConnection } from '../../core/types';
import { 
  Cable as CableIcon, 
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon 
} from '@mui/icons-material';

interface ConnectionFormData extends DatabaseConnection {
  saveConnection?: boolean;
}

export const ConnectionPage = () => {
  const navigate = useNavigate();
  const [connectionStatus, setConnectionStatus] = useState<{
    success: boolean;
    message: string;
  } | null>(null);

  const { control, handleSubmit, watch, formState: { errors } } = useForm<ConnectionFormData>({
    defaultValues: {
      databaseType: 0,
      server: '',
      port: undefined,
      database: '',
      username: '',
      password: '',
      trustServerCertificate: true,
      saveConnection: false,
    },
  });

  const databaseType = watch('databaseType');

  const testMutation = useMutation({
    mutationFn: databaseService.testConnection,
    onSuccess: (data) => {
      setConnectionStatus({
        success: true,
        message: data.message || 'Connection successful!',
      });
    },
    onError: (error: Error) => {
      setConnectionStatus({
        success: false,
        message: error.message || 'Connection failed',
      });
    },
  });

  const connectMutation = useMutation({
    mutationFn: databaseService.connect,
    onSuccess: (data) => {
      setConnectionStatus({
        success: true,
        message: 'Connected successfully! Redirecting to Database Explorer...',
      });
      
      // Navigate to Database Explorer after 1.5 seconds
      setTimeout(() => {
        navigate('/explorer', { 
          state: { 
            connected: true,
            databaseName: data.databaseName,
            serverVersion: data.serverVersion 
          } 
        });
      }, 1500);
    },
    onError: (error: Error) => {
      setConnectionStatus({
        success: false,
        message: error.message || 'Failed to establish connection',
      });
    },
  });

  const onTestConnection = (data: ConnectionFormData) => {
    setConnectionStatus(null);
    const connectionData = { ...data };
    delete connectionData.saveConnection;
    testMutation.mutate(connectionData);
  };

  const onConnect = (data: ConnectionFormData) => {
    setConnectionStatus(null);
    const connectionData = { ...data };
    delete connectionData.saveConnection;
    connectMutation.mutate(connectionData);
  };

  const getDefaultPort = () => {
    return databaseType === 0 ? 1433 : 3306;
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <CableIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
          <Box>
            <Typography variant="h4" component="h1" fontWeight={600}>
              Database Connection
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
              Configure and establish database connections
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {connectionStatus && (
          <Alert 
            severity={connectionStatus.success ? 'success' : 'error'} 
            sx={{ mb: 3 }}
            icon={connectionStatus.success ? <CheckCircleIcon /> : <ErrorIcon />}
          >
            {connectionStatus.message}
          </Alert>
        )}

        <Card elevation={2}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h6" gutterBottom fontWeight={600}>
              Connection Details
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Enter your database credentials to establish a connection
            </Typography>

            <Box component="form" sx={{ mt: 3 }}>
              <Stack spacing={3}>
                {/* Database Type */}
                <Controller
                  name="databaseType"
                  control={control}
                  rules={{ required: 'Database type is required' }}
                  render={({ field }) => (
                    <FormControl fullWidth error={!!errors.databaseType}>
                      <InputLabel>Database Type *</InputLabel>
                      <Select {...field} label="Database Type *">
                        <MenuItem value={0}>SQL Server</MenuItem>
                        <MenuItem value={1}>MySQL</MenuItem>
                      </Select>
                      {errors.databaseType && (
                        <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 1.5 }}>
                          {errors.databaseType.message}
                        </Typography>
                      )}
                    </FormControl>
                  )}
                />

                {/* Server and Port */}
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
                  <Controller
                    name="server"
                    control={control}
                    rules={{ 
                      required: 'Server address is required',
                      pattern: {
                        value: /^[a-zA-Z0-9.-]+$/,
                        message: 'Invalid server address'
                      }
                    }}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        label="Server *"
                        placeholder={databaseType === 0 ? 'localhost or server-name' : 'localhost'}
                        fullWidth
                        error={!!errors.server}
                        helperText={errors.server?.message}
                      />
                    )}
                  />

                  <Controller
                    name="port"
                    control={control}
                    render={({ field: { value, onChange, ...rest } }) => (
                      <TextField
                        {...rest}
                        value={value || ''}
                        onChange={(e) => {
                          const val = e.target.value;
                          onChange(val ? Number(val) : undefined);
                        }}
                        label="Port"
                        placeholder={`Default: ${getDefaultPort()}`}
                        type="number"
                        fullWidth
                        error={!!errors.port}
                        helperText={errors.port?.message || `Default port will be used if not specified`}
                        sx={{ maxWidth: { md: 200 } }}
                      />
                    )}
                  />
                </Stack>

                {/* Database Name */}
                <Controller
                  name="database"
                  control={control}
                  rules={{ 
                    required: 'Database name is required',
                    minLength: {
                      value: 1,
                      message: 'Database name is required'
                    }
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Database *"
                      placeholder="Enter database name"
                      fullWidth
                      error={!!errors.database}
                      helperText={errors.database?.message}
                    />
                  )}
                />

                {/* Username and Password */}
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
                  <Controller
                    name="username"
                    control={control}
                    render={({ field }) => (
                      <TextField 
                        {...field} 
                        label="Username" 
                        placeholder="Database username"
                        fullWidth 
                        error={!!errors.username}
                        helperText={errors.username?.message}
                      />
                    )}
                  />

                  <Controller
                    name="password"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        label="Password"
                        type="password"
                        placeholder="Database password"
                        fullWidth
                        error={!!errors.password}
                        helperText={errors.password?.message}
                      />
                    )}
                  />
                </Stack>

                {/* Trust Server Certificate (SQL Server only) */}
                {databaseType === 0 && (
                  <Box sx={{ pt: 1 }}>
                    <Controller
                      name="trustServerCertificate"
                      control={control}
                      render={({ field }) => (
                        <FormControlLabel
                          control={
                            <Switch 
                              {...field} 
                              checked={field.value} 
                              color="primary"
                            />
                          }
                          label={
                            <Box>
                              <Typography variant="body2">
                                Trust Server Certificate
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                Required for self-signed certificates
                              </Typography>
                            </Box>
                          }
                        />
                      )}
                    />
                  </Box>
                )}

                {/* Action Buttons */}
                <Divider sx={{ my: 2 }} />
                
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  <Button
                    variant="outlined"
                    size="large"
                    onClick={handleSubmit(onTestConnection)}
                    disabled={testMutation.isPending || connectMutation.isPending}
                    startIcon={testMutation.isPending ? <CircularProgress size={20} /> : null}
                    sx={{ minWidth: 180 }}
                  >
                    {testMutation.isPending ? 'Testing...' : 'Test Connection'}
                  </Button>
                  
                  <Button
                    variant="contained"
                    size="large"
                    onClick={handleSubmit(onConnect)}
                    disabled={connectMutation.isPending || testMutation.isPending}
                    startIcon={connectMutation.isPending ? <CircularProgress size={20} /> : null}
                    sx={{ minWidth: 180 }}
                  >
                    {connectMutation.isPending ? 'Connecting...' : 'Connect'}
                  </Button>
                </Box>

                <Typography variant="caption" color="text.secondary" sx={{ pt: 1 }}>
                  * Required fields
                </Typography>
              </Stack>
            </Box>
          </CardContent>
        </Card>

        {/* Connection Tips */}
        <Card sx={{ mt: 3 }} variant="outlined">
          <CardContent>
            <Typography variant="subtitle2" fontWeight={600} gutterBottom>
              Connection Tips
            </Typography>
            <Stack spacing={1} sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                • <strong>SQL Server:</strong> Use server name or IP address. Default port is 1433.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                • <strong>MySQL:</strong> Use hostname or IP address. Default port is 3306.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                • <strong>Authentication:</strong> Windows Authentication or SQL Server Authentication for SQL Server.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                • <strong>Test Connection:</strong> Verify credentials before connecting.
              </Typography>
            </Stack>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};
