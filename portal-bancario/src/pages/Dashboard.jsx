import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Container, Typography, Box, Button, AppBar, Toolbar, Card, CardContent, CircularProgress, Alert
} from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import ListAltIcon from '@mui/icons-material/ListAlt';
import SendIcon from '@mui/icons-material/Send';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import { jwtDecode } from 'jwt-decode';

function Dashboard() {
  const navigate = useNavigate();
  const [saldo, setSaldo] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [usuario, setUsuario] = useState('');
  const [cuentaId, setCuentaId] = useState('');

  const token = localStorage.getItem('jwt_token');

  useEffect(() => {
    if (!token) {
      navigate('/login');
      return;
    }

    let timeoutId;

    try {
      const decoded = jwtDecode(token);
      
      // 1. Verificar si el token ya expiró
      const currentTime = Date.now() / 1000;
      if (decoded.exp < currentTime) {
        console.warn('El token ha expirado. Redirigiendo al login...');
        localStorage.removeItem('jwt_token');
        navigate('/login');
        return;
      }

      // 2. Programar cierre de sesión automático cuando expire el token
      const timeRemainingMs = (decoded.exp - currentTime) * 1000;
      timeoutId = setTimeout(() => {
        alert('Tu sesión ha expirado por seguridad.');
        localStorage.removeItem('jwt_token');
        navigate('/login');
      }, timeRemainingMs);

      // Extraer datos de los claims estándar de ASP.NET Core
      const idCuenta = decoded['IdCuenta'] || decoded.IdCuenta;
      const nombreUsuario = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decoded.unique_name || 'Usuario';
      const propietario = decoded['Propietario'] || nombreUsuario;
      
      setCuentaId(idCuenta);
      setUsuario(propietario);

      if (idCuenta) {
        fetchSaldo(idCuenta);
      } else {
        setError('El token no contiene el identificador de la cuenta.');
        setLoading(false);
      }
    } catch (err) {
      console.error("Error decodificando el token:", err);
      localStorage.removeItem('jwt_token');
      navigate('/login');
    }

    return () => {
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [navigate, token]);

  const fetchSaldo = async (idCuenta) => {
    try {
      // El microservicio WebApi corre en el puerto 7125 (https)
      const response = await fetch(`https://localhost:7125/api/Cuenta/${idCuenta}/saldo`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.status === 401) {
        localStorage.removeItem('jwt_token');
        navigate('/login');
        return;
      }

      if (!response.ok) {
        throw new Error('Error al obtener el saldo');
      }

      const data = await response.json();
      setSaldo(data); 
    } catch (err) {
      setError('No se pudo cargar el saldo actual. Verifica que la WebApi esté corriendo.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleVerMovimientos = () => {
    navigate('/movimientos');
  };

  const handleTransferir = () => {
    navigate('/transferencia');
  };

  const handleLogout = () => {
    localStorage.removeItem('jwt_token');
    navigate('/login');
  };

  // Determinar si la respuesta fue un numero plano o un objeto
  const saldoFormateado = typeof saldo === 'number' ? saldo : (saldo?.saldoActual ?? 0);

  return (
    <Box sx={{ flexGrow: 1, minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" color="primary" elevation={2}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
            BancoApp
          </Typography>
          <Button color="inherit" onClick={handleLogout} endIcon={<LogoutIcon />}>
            Cerrar Sesión
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="md" sx={{ mt: 6 }}>
        <Typography variant="h4" gutterBottom fontWeight="bold" color="text.primary">
          Hola, {usuario}
        </Typography>
        <Typography variant="subtitle1" color="text.secondary" mb={4}>
          Aquí tienes el resumen de tu cuenta bancaria.
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        <Card sx={{ p: 2, mb: 4, display: 'flex', alignItems: 'center', justifyContent: 'space-between', bgcolor: 'background.paper' }}>
          <Box display="flex" alignItems="center">
            <Box sx={{ bgcolor: 'primary.dark', p: 2, borderRadius: 2, mr: 3 }}>
              <AccountBalanceWalletIcon sx={{ fontSize: 40, color: 'primary.contrastText' }} />
            </Box>
            <Box>
              <Typography variant="overline" color="text.secondary">
                Saldo Actual
              </Typography>
              {loading ? (
                <CircularProgress size={24} sx={{ display: 'block', mt: 1 }} />
              ) : (
                <Typography variant="h3" fontWeight="bold" color="text.primary">
                  ${saldoFormateado.toLocaleString('es-MX', { minimumFractionDigits: 2 })}
                </Typography>
              )}
            </Box>
          </Box>
          <Box display="flex" gap={2}>
            <Button 
              variant="contained" 
              color="primary" 
              startIcon={<SendIcon />}
              onClick={handleTransferir}
              disabled={loading || !cuentaId}
              size="large"
            >
              Transferir
            </Button>
            <Button 
              variant="outlined" 
              color="secondary" 
              startIcon={<ListAltIcon />}
              onClick={handleVerMovimientos}
              disabled={loading || !cuentaId}
              size="large"
            >
              Historial
            </Button>
          </Box>
        </Card>

      </Container>
    </Box>
  );
}

export default Dashboard;
