import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  CircularProgress,
  Alert,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Button,
  Stack,
} from '@mui/material';
import {
  Explore as ExploreIcon,
  ExpandMore as ExpandMoreIcon,
  TableChart,
  ViewList,
  FolderSpecial,
  Functions as FunctionsIcon,
  Key as KeyIcon,
  Link as LinkIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';

export const ExplorerPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [connection, setConnection] = useState<any>(null);
  const [expanded, setExpanded] = useState<string | false>(false);

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (currentConnection) {
      setConnection(currentConnection);
    }
  }, [location]);

  const { data: tables, isLoading: loadingTables, refetch: refetchTables } = useQuery({
    queryKey: ['tables', connection],
    queryFn: () => connection ? databaseService.getTables(connection) : Promise.resolve([]),
    enabled: !!connection,
  });

  const { data: views, isLoading: loadingViews, refetch: refetchViews } = useQuery({
    queryKey: ['views', connection],
    queryFn: () => connection ? databaseService.getViews(connection) : Promise.resolve([]),
    enabled: !!connection,
  });

  const { data: procedures, isLoading: loadingProcedures, refetch: refetchProcedures } = useQuery({
    queryKey: ['procedures', connection],
    queryFn: () => connection ? databaseService.getStoredProcedures(connection) : Promise.resolve([]),
    enabled: !!connection,
  });

  const handleChange = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
    setExpanded(isExpanded ? panel : false);
  };

  const handleRefreshAll = () => {
    refetchTables();
    refetchViews();
    refetchProcedures();
  };

  if (!connection) {
    return (
      <Container maxWidth="xl">
        <Box sx={{ py: 4 }}>
          <Alert severity="warning">
            No database connection found. Please connect to a database first.
            <Button
              variant="outlined"
              size="small"
              onClick={() => navigate('/connection')}
              sx={{ ml: 2 }}
            >
              Go to Connection Page
            </Button>
          </Alert>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <ExploreIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
            <Box>
              <Typography variant="h4" component="h1" fontWeight={600}>
                Database Explorer
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                Browse and explore database objects
              </Typography>
            </Box>
          </Box>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefreshAll}
          >
            Refresh All
          </Button>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {/* Tables Accordion */}
        <Accordion expanded={expanded === 'tables'} onChange={handleChange('tables')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="tables-content"
            id="tables-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <TableChart color="primary" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Tables</Typography>
              <Chip
                label={tables?.length || 0}
                size="small"
                color="primary"
                variant="outlined"
              />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            {loadingTables ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                <CircularProgress />
              </Box>
            ) : tables && tables.length > 0 ? (
              <List>
                {tables.map((table: any, index: number) => (
                  <ListItem key={index}>
                    <ListItemIcon>
                      <TableChart fontSize="small" />
                    </ListItemIcon>
                    <ListItemText
                      primary={table.tableName}
                      secondary={table.schemaName ? `Schema: ${table.schemaName}` : undefined}
                    />
                    {table.rowCount !== undefined && (
                      <Chip label={`${table.rowCount} rows`} size="small" variant="outlined" />
                    )}
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                No tables found
              </Typography>
            )}
          </AccordionDetails>
        </Accordion>

        {/* Views Accordion */}
        <Accordion expanded={expanded === 'views'} onChange={handleChange('views')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="views-content"
            id="views-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <ViewList color="secondary" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Views</Typography>
              <Chip
                label={views?.length || 0}
                size="small"
                color="secondary"
                variant="outlined"
              />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            {loadingViews ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                <CircularProgress />
              </Box>
            ) : views && views.length > 0 ? (
              <List>
                {views.map((view: any, index: number) => (
                  <ListItem key={index}>
                    <ListItemIcon>
                      <ViewList fontSize="small" />
                    </ListItemIcon>
                    <ListItemText
                      primary={view.viewName}
                      secondary={view.schemaName ? `Schema: ${view.schemaName}` : undefined}
                    />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                No views found
              </Typography>
            )}
          </AccordionDetails>
        </Accordion>

        {/* Stored Procedures Accordion */}
        <Accordion expanded={expanded === 'procedures'} onChange={handleChange('procedures')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="procedures-content"
            id="procedures-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <FolderSpecial color="success" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Stored Procedures</Typography>
              <Chip
                label={procedures?.length || 0}
                size="small"
                color="success"
                variant="outlined"
              />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            {loadingProcedures ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                <CircularProgress />
              </Box>
            ) : procedures && procedures.length > 0 ? (
              <Box>
                <List>
                  {procedures.slice(0, 10).map((proc: any, index: number) => (
                    <ListItem key={index}>
                      <ListItemIcon>
                        <FolderSpecial fontSize="small" />
                      </ListItemIcon>
                      <ListItemText
                        primary={proc.name}
                        secondary={proc.schemaName ? `Schema: ${proc.schemaName}` : undefined}
                      />
                    </ListItem>
                  ))}
                </List>
                {procedures.length > 10 && (
                  <Button
                    variant="text"
                    onClick={() => navigate('/procedures')}
                    sx={{ mt: 2 }}
                  >
                    View All {procedures.length} Procedures
                  </Button>
                )}
              </Box>
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                No stored procedures found
              </Typography>
            )}
          </AccordionDetails>
        </Accordion>

        {/* Functions Accordion */}
        <Accordion expanded={expanded === 'functions'} onChange={handleChange('functions')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="functions-content"
            id="functions-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <FunctionsIcon color="warning" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Functions</Typography>
              <Chip label={0} size="small" color="warning" variant="outlined" />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
              No functions found
            </Typography>
          </AccordionDetails>
        </Accordion>

        {/* Indexes Accordion */}
        <Accordion expanded={expanded === 'indexes'} onChange={handleChange('indexes')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="indexes-content"
            id="indexes-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <KeyIcon color="info" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Indexes</Typography>
              <Chip label={0} size="small" color="info" variant="outlined" />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
              No indexes information available
            </Typography>
          </AccordionDetails>
        </Accordion>

        {/* Foreign Keys Accordion */}
        <Accordion expanded={expanded === 'foreignkeys'} onChange={handleChange('foreignkeys')}>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon />}
            aria-controls="foreignkeys-content"
            id="foreignkeys-header"
          >
            <Stack direction="row" spacing={2} alignItems="center" sx={{ width: '100%', pr: 2 }}>
              <LinkIcon color="error" />
              <Typography variant="h6" sx={{ flexGrow: 1 }}>Foreign Keys</Typography>
              <Chip label={0} size="small" color="error" variant="outlined" />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
              No foreign keys information available
            </Typography>
          </AccordionDetails>
        </Accordion>
      </Box>
    </Container>
  );
};
