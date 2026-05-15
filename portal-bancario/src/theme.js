import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'dark', // Tema oscuro premium
    primary: {
      main: '#90caf9', // Azul claro para el modo oscuro
    },
    secondary: {
      main: '#f48fb1',
    },
    background: {
      default: '#0a1929', // Fondo azul oscuro casi negro
      paper: '#001e3c',   // Fondo para las tarjetas/papers
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none', // Botones más modernos sin todo en mayúscula
          borderRadius: 8,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 16,
          boxShadow: '0 8px 16px 0 rgba(0,0,0,0.4)',
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 8,
          }
        }
      }
    }
  },
});

export default theme;
