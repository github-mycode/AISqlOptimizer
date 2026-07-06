import { useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Container,
  FormControl,
  FormControlLabel,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Switch,
  TextField,
  Typography,
  Alert,
  Stack,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { useMutation } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';
import type { ConnectionResponse } from '../../core/services/database.service';
import type { DatabaseConnection } from '../../core/types';
import { Loading } from '../../core/components/Loading';

export const HomePage = () => {
  const [connectionResult, setConnectionResult] = useState<ConnectionResponse | null>(null);

  const { control, handleSubmit, watch } = useForm<DatabaseConnection>({
    defaultValues: {
      databaseType: 0,
      server: 'localhost',
      database: '',
      username: '',
      password: '',
      trustServerCertificate: true,
    },
  });

  const databaseType = watch('databaseType');

  const connectionMutation = useMutation({
    mutationFn: (data: DatabaseConnection) => databaseService.testConnection(data),
    onSuccess: (data) => {
      setConnectionResult(data);
    },
    onError: (error: any) => {
      setConnectionResult({
        success: false,
        message: error.message || 'Connection failed',
        databaseType: '',
        serverVersion: '',
        databaseName: '',
        timestamp: new Date().toISOString(),
      });
    },
  });

  const onSubmit = (data: DatabaseConnection) => {
    setConnectionResult(null);
    connectionMutation.mutate(data);
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Typography variant="h3" gutterBottom>
          Welcome to SQL Optimizer
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
          Connect to your database to analyze and optimize SQL queries and stored procedures.
        </Typography>

        <Card sx={{ mt: 4 }}>
          <CardContent>
            <Typography variant="h5" gutterBottom>
              Database Connection
            </Typography>

            <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 3 }}>
              <Stack spacing={3}>
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
                  <Controller
                    name="databaseType"
                    control={control}
                    render={({ field }) => (
                      <FormControl fullWidth>
                        <InputLabel>Database Type</InputLabel>
                        <Select {...field} label="Database Type">
                          <MenuItem value={0}>SQL Server</MenuItem>
                          <MenuItem value={1}>MySQL</MenuItem>
                        </Select>
                      </FormControl>
                    )}
                  />
                </Stack>

                <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
                  <Controller
                    name="server"
                    control={control}
                    rules={{ required: 'Server is required' }}
                    render={({ field, fieldState }) => (
                      <TextField
                        {...field}
                        label="Server"
                        fullWidth
                        error={!!fieldState.error}
                        helperText={fieldState.error?.message}
                      />
                    )}
                  />

                  <Controller
                    name="database"
                    control={control}
                    rules={{ required: 'Database is required' }}
                    render={({ field, fieldState }) => (
                      <TextField
                        {...field}
                        label="Database"
                        fullWidth
                        error={!!fieldState.error}
                        helperText={fieldState.error?.message}
                      />
                    )}
                  />
                </Stack>

                <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
                  <Controller
                    name="username"
                    control={control}
                    render={({ field }) => (
                      <TextField {...field} label="Username" fullWidth />
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
                        fullWidth
                      />
                    )}
                  />
                </Stack>

                {databaseType === 0 && (
                  <Box>
                    <Controller
                      name="trustServerCertificate"
                      control={control}
                      render={({ field }) => (
                        <FormControlLabel
                          control={<Switch {...field} checked={field.value} />}
                          label="Trust Server Certificate"
                        />
                      )}
                    />
                  </Box>
                )}

                <Box>
                  <Button
                    type="submit"
                    variant="contained"
                    size="large"
                    disabled={connectionMutation.isPending}
                  >
                    {connectionMutation.isPending ? 'Connecting...' : 'Test Connection'}
                  </Button>
                </Box>
              </Stack>
            </Box>

            {connectionMutation.isPending && (
              <Box sx={{ mt: 3 }}>
                <Loading message="Testing connection..." />
              </Box>
            )}

            {connectionResult && (
              <Box sx={{ mt: 3 }}>
                <Alert severity={connectionResult.success ? 'success' : 'error'}>
                  <Typography variant="subtitle1" gutterBottom>
                    {connectionResult.message}
                  </Typography>
                  {connectionResult.success && (
                    <>
                      <Typography variant="body2">
                        Database Type: {connectionResult.databaseType}
                      </Typography>
                      <Typography variant="body2">
                        Server Version: {connectionResult.serverVersion}
                      </Typography>
                      <Typography variant="body2">
                        Database Name: {connectionResult.databaseName}
                      </Typography>
                    </>
                  )}
                </Alert>
              </Box>
            )}
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};
