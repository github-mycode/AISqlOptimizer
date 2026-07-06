import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  Card,
  CardContent,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Switch,
  FormControlLabel,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Fade,
  Grid,
} from '@mui/material';
import {
  Settings as SettingsIcon,
  Save as SaveIcon,
  RestartAlt as ResetIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import { useThemeMode } from '../../core/theme/ThemeProvider';

interface SettingsData {
  backendUrl: string;
  openaiModel: string;
  theme: 'light' | 'dark';
  language: string;
  autoSave: boolean;
  notifications: boolean;
}

const defaultSettings: SettingsData = {
  backendUrl: 'http://localhost:5119/api',
  openaiModel: 'gpt-4',
  theme: 'light',
  language: 'en',
  autoSave: true,
  notifications: true,
};

export const SettingsPage = () => {
  const { mode, toggleTheme } = useThemeMode();
  const [settings, setSettings] = useState<SettingsData>(defaultSettings);
  const [saved, setSaved] = useState(false);
  const [resetDialogOpen, setResetDialogOpen] = useState(false);

  useEffect(() => {
    // Load settings from localStorage
    const savedSettings = localStorage.getItem('appSettings');
    if (savedSettings) {
      setSettings(JSON.parse(savedSettings));
    } else {
      setSettings({ ...defaultSettings, theme: mode });
    }
  }, []);

  const handleSave = () => {
    // Save to localStorage
    localStorage.setItem('appSettings', JSON.stringify(settings));
    
    // Apply theme if changed
    if (settings.theme !== mode) {
      toggleTheme();
    }

    // Update environment
    if (typeof window !== 'undefined') {
      (window as any).VITE_API_BASE_URL = settings.backendUrl;
    }

    setSaved(true);
    setTimeout(() => setSaved(false), 3000);
  };

  const handleReset = () => {
    setSettings({ ...defaultSettings, theme: 'light' });
    localStorage.removeItem('appSettings');
    setResetDialogOpen(false);
    
    // Reset theme to light
    if (mode === 'dark') {
      toggleTheme();
    }
  };

  const handleChange = (field: keyof SettingsData, value: any) => {
    setSettings(prev => ({ ...prev, [field]: value }));
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        {/* Header */}
        <Fade in timeout={500}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <SettingsIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
                <Box>
                  <Typography variant="h4" component="h1" fontWeight={600}>
                    Settings
                  </Typography>
                  <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                    Configure application preferences
                  </Typography>
                </Box>
              </Box>
              <Stack direction="row" spacing={2}>
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<ResetIcon />}
                  onClick={() => setResetDialogOpen(true)}
                >
                  Reset
                </Button>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={handleSave}
                >
                  Save Settings
                </Button>
              </Stack>
            </Box>

            <Divider sx={{ mb: 4 }} />
          </Box>
        </Fade>

        {/* Success Message */}
        {saved && (
          <Fade in>
            <Alert 
              severity="success" 
              icon={<CheckCircleIcon />}
              sx={{ mb: 3 }}
            >
              Settings saved successfully!
            </Alert>
          </Fade>
        )}

        {/* Settings Sections */}
        <Stack spacing={3}>
          {/* Backend Configuration */}
          <Fade in timeout={700}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  Backend Configuration
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Configure your backend API settings
                </Typography>
                <Stack spacing={3}>
                  <TextField
                    label="Backend URL"
                    value={settings.backendUrl}
                    onChange={(e) => handleChange('backendUrl', e.target.value)}
                    fullWidth
                    helperText="API base URL for backend services"
                  />
                  <FormControl fullWidth>
                    <InputLabel>OpenAI Model</InputLabel>
                    <Select
                      value={settings.openaiModel}
                      label="OpenAI Model"
                      onChange={(e) => handleChange('openaiModel', e.target.value)}
                    >
                      <MenuItem value="gpt-3.5-turbo">GPT-3.5 Turbo</MenuItem>
                      <MenuItem value="gpt-4">GPT-4</MenuItem>
                      <MenuItem value="gpt-4-turbo">GPT-4 Turbo</MenuItem>
                      <MenuItem value="gpt-4o">GPT-4o</MenuItem>
                    </Select>
                  </FormControl>
                </Stack>
              </CardContent>
            </Card>
          </Fade>

          {/* Appearance */}
          <Fade in timeout={900}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  Appearance
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Customize the look and feel
                </Typography>
                <Grid container spacing={3}>
                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Theme</InputLabel>
                      <Select
                        value={settings.theme}
                        label="Theme"
                        onChange={(e) => handleChange('theme', e.target.value)}
                      >
                        <MenuItem value="light">Light</MenuItem>
                        <MenuItem value="dark">Dark</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                      <InputLabel>Language</InputLabel>
                      <Select
                        value={settings.language}
                        label="Language"
                        onChange={(e) => handleChange('language', e.target.value)}
                      >
                        <MenuItem value="en">English</MenuItem>
                        <MenuItem value="es">Spanish</MenuItem>
                        <MenuItem value="fr">French</MenuItem>
                        <MenuItem value="de">German</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Fade>

          {/* Preferences */}
          <Fade in timeout={1100}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  Preferences
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Application behavior settings
                </Typography>
                <Stack spacing={2}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={settings.autoSave}
                        onChange={(e) => handleChange('autoSave', e.target.checked)}
                      />
                    }
                    label={
                      <Box>
                        <Typography variant="body1">Auto-save</Typography>
                        <Typography variant="caption" color="text.secondary">
                          Automatically save changes without confirmation
                        </Typography>
                      </Box>
                    }
                  />
                  <FormControlLabel
                    control={
                      <Switch
                        checked={settings.notifications}
                        onChange={(e) => handleChange('notifications', e.target.checked)}
                      />
                    }
                    label={
                      <Box>
                        <Typography variant="body1">Notifications</Typography>
                        <Typography variant="caption" color="text.secondary">
                          Show notifications for important events
                        </Typography>
                      </Box>
                    }
                  />
                </Stack>
              </CardContent>
            </Card>
          </Fade>

          {/* Storage Info */}
          <Fade in timeout={1300}>
            <Card sx={{ bgcolor: 'info.50', border: '1px solid', borderColor: 'info.light' }}>
              <CardContent>
                <Typography variant="subtitle2" fontWeight={600} color="info.main" gutterBottom>
                  ℹ️ Storage Information
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  All settings are stored locally in your browser's localStorage. No data is sent to external servers. Clearing your browser data will reset these settings to default values.
                </Typography>
              </CardContent>
            </Card>
          </Fade>
        </Stack>

        {/* Reset Confirmation Dialog */}
        <Dialog open={resetDialogOpen} onClose={() => setResetDialogOpen(false)}>
          <DialogTitle>Reset Settings?</DialogTitle>
          <DialogContent>
            <Typography>
              Are you sure you want to reset all settings to default values? This action cannot be undone.
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setResetDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleReset} color="error" variant="contained">
              Reset
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Container>
  );
};
