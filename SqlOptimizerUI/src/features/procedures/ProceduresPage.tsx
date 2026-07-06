import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  TextField,
  InputAdornment,
  Alert,
  CircularProgress,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Code as CodeIcon,
  Refresh as RefreshIcon,
  Search as SearchIcon,
  Visibility as ViewIcon,
  Psychology as AnalyzeIcon,
  Speed as OptimizeIcon,
} from '@mui/icons-material';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { useQuery } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';
import type { StoredProcedure } from '../../core/types';

export const ProceduresPage = () => {
  const navigate = useNavigate();
  const [connection, setConnection] = useState<any>(null);
  const [searchText, setSearchText] = useState('');
  const [paginationModel, setPaginationModel] = useState({
    page: 0,
    pageSize: 10,
  });

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (currentConnection) {
      setConnection(currentConnection);
    } else {
      // Set a mock connection for development
      const mockConnection = {
        databaseType: 0 as 0 | 1,
        server: 'localhost',
        database: 'TestDB',
      };
      setConnection(mockConnection);
    }
  }, []);

  const { data: procedures, isLoading, refetch } = useQuery({
    queryKey: ['procedures', connection],
    queryFn: async () => {
      if (!connection) return [];
      const procs = await databaseService.getStoredProcedures(connection);
      console.log('Fetched procedures in component:', procs);
      return procs;
    },
    enabled: !!connection,
  });

  const handleView = (procedureName: string | undefined) => {
    if (!procedureName) {
      console.error('Procedure name is undefined');
      return;
    }
    navigate(`/procedures/${encodeURIComponent(procedureName)}`);
  };

  const handleAnalyze = (procedureName: string | undefined) => {
    if (!procedureName) {
      console.error('Procedure name is undefined');
      alert('Cannot analyze procedure: procedure name is missing');
      return;
    }
    console.log('Navigating to AI analysis with procedure:', procedureName);
    // Navigate to AI analysis page with procedure context
    navigate('/ai-analysis', { state: { procedureName } });
  };

  const handleOptimize = (procedureName: string | undefined) => {
    if (!procedureName) {
      console.error('Procedure name is undefined');
      return;
    }
    // Navigate to AI analysis with optimize mode
    navigate('/ai-analysis', { state: { procedureName, mode: 'optimize' } });
  };

  const columns: GridColDef[] = [
    {
      field: 'name',
      headerName: 'Name',
      flex: 1,
      minWidth: 200,
    },
    {
      field: 'schemaName',
      headerName: 'Schema',
      width: 150,
    },
    {
      field: 'createDate',
      headerName: 'Created Date',
      width: 180,
      valueFormatter: (value) => {
        if (!value) return 'N/A';
        return new Date(value).toLocaleDateString();
      },
    },
    {
      field: 'modifyDate',
      headerName: 'Modified Date',
      width: 180,
      valueFormatter: (value) => {
        if (!value) return 'N/A';
        return new Date(value).toLocaleDateString();
      },
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 180,
      sortable: false,
      renderCell: (params) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View">
            <IconButton
              size="small"
              color="primary"
              onClick={() => handleView(params.row.name)}
              disabled={!params.row.name}
            >
              <ViewIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Analyze">
            <IconButton
              size="small"
              color="secondary"
              onClick={() => handleAnalyze(params.row.name)}
              disabled={!params.row.name}
            >
              <AnalyzeIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Optimize">
            <IconButton
              size="small"
              color="success"
              onClick={() => handleOptimize(params.row.name)}
              disabled={!params.row.name}
            >
              <OptimizeIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  const filteredProcedures = procedures?.filter((proc: StoredProcedure) =>
    (proc.name && proc.name.toLowerCase().includes(searchText.toLowerCase())) ||
    (proc.schemaName && proc.schemaName.toLowerCase().includes(searchText.toLowerCase()))
  ) || [];

  // Always render the page content
  return (
    <Container maxWidth="xl">    <Box sx={{ py: 4 }}>
        {!databaseService.getCurrentConnection() && (
          <Alert severity="info" sx={{ mb: 3 }}>
            No database connection. Showing mock data for demonstration.
            <Button
              variant="outlined"
              size="small"
              onClick={() => navigate('/connection')}
              sx={{ ml: 2 }}
            >
              Connect to Database
            </Button>
          </Alert>
        )}
        
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <CodeIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
            <Box>
              <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
                Stored Procedures
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                Manage and analyze stored procedures
              </Typography>
            </Box>
          </Box>
          <Button
            variant="outlined"
            startIcon={isLoading ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={() => refetch()}
            disabled={isLoading}
          >
            Refresh
          </Button>
        </Box>

        <Divider sx={{ mb: 4 }} />

        <Stack spacing={3}>
          {/* Search Bar */}
          <TextField
            placeholder="Search procedures..."
            variant="outlined"
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              },
            }}
            sx={{ maxWidth: 400 }}
          />

          {/* DataGrid */}
          <Box sx={{ height: 600, width: '100%' }}>
            {procedures && procedures.length > 0 && (
              <Alert severity="info" sx={{ mb: 2 }}>
                Found {procedures.length} procedures. 
                {procedures[0] && (
                  <span> Sample data: {JSON.stringify(Object.keys(procedures[0]))}</span>
                )}
              </Alert>
            )}
            <DataGrid
              rows={filteredProcedures.map((proc: StoredProcedure, index: number) => {
                console.log('Procedure row:', proc);
                const procAny = proc as any;
                return {
                  id: index,
                  name: proc.name || procAny.procedureName || procAny.ROUTINE_NAME || 'Unknown',
                  schemaName: proc.schemaName || procAny.schema || procAny.ROUTINE_SCHEMA || 'dbo',
                  createDate: proc.createDate || procAny.CREATED,
                  modifyDate: proc.modifyDate || procAny.LAST_ALTERED,
                };
              })}
              columns={columns}
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[5, 10, 25, 50, 100]}
              loading={isLoading}
              disableRowSelectionOnClick
              sx={{
                '& .MuiDataGrid-cell:focus': {
                  outline: 'none',
                },
                '& .MuiDataGrid-row:hover': {
                  backgroundColor: 'action.hover',
                },
              }}
            />
          </Box>
        </Stack>
      </Box>
    </Container>
  );
};
