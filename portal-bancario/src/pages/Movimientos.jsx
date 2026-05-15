import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Container, Typography, Box, Button, AppBar, Toolbar, CircularProgress, Alert,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import DownloadIcon from '@mui/icons-material/Download';
import { jwtDecode } from 'jwt-decode';

function Movimientos() {
  const navigate = useNavigate();
  const [datos, setDatos] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cuentaId, setCuentaId] = useState('');

  const token = localStorage.getItem('jwt_token');

  useEffect(() => {
    if (!token) {
      navigate('/login');
      return;
    }

    try {
      const decoded = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      if (decoded.exp < currentTime) {
        localStorage.removeItem('jwt_token');
        navigate('/login');
        return;
      }

      const idCuenta = decoded['IdCuenta'] || decoded.IdCuenta;
      setCuentaId(idCuenta);

      if (idCuenta) {
        fetchMovimientos(idCuenta);
      } else {
        setError('El token no contiene el identificador de la cuenta.');
        setLoading(false);
      }
    } catch (err) {
      console.error("Error:", err);
      localStorage.removeItem('jwt_token');
      navigate('/login');
    }
  }, [navigate, token]);

  const fetchMovimientos = async (id) => {
    try {
      const response = await fetch(`https://localhost:7125/api/Cuenta/${id}/estado-cuenta`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.status === 401) {
        localStorage.removeItem('jwt_token');
        navigate('/login');
        return;
      }
      if (response.status === 404) {
        // En tu backend esto devuelve 404 si el saldo es 0.00m o no hay datos
        setDatos({ saldo: 0, movimientos: [] });
        return;
      }
      if (!response.ok) {
        throw new Error('Error al obtener los movimientos');
      }

      const data = await response.json();
      setDatos(data);
    } catch (err) {
      setError('No se pudieron cargar los movimientos.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDescargarPDF = async () => {
    try {
      const response = await fetch(`https://localhost:7125/api/Cuenta/${cuentaId}/estado-cuenta-pdf`, {
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
        throw new Error('Error al descargar el PDF');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `EstadoCuenta_${cuentaId}.pdf`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert('Error al descargar el estado de cuenta.');
      console.error(err);
    }
  };

  return (
    <Box sx={{ flexGrow: 1, minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" color="primary" elevation={2}>
        <Toolbar>
          <Button color="inherit" onClick={() => navigate('/dashboard')} startIcon={<ArrowBackIcon />}>
            Volver
          </Button>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold', ml: 2 }}>
            Movimientos
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="md" sx={{ mt: 6 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
          <Box>
            <Typography variant="h4" fontWeight="bold" color="text.primary">
              Historial de Cuenta
            </Typography>
            <Typography variant="subtitle1" color="text.secondary">
              Revisa tus últimos movimientos
            </Typography>
          </Box>
          <Button 
            variant="contained" 
            color="secondary" 
            startIcon={<DownloadIcon />}
            onClick={handleDescargarPDF}
            disabled={loading || !cuentaId || !datos || datos.movimientos?.length === 0}
          >
            Estado de Cuenta
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        {loading ? (
          <Box display="flex" justifyContent="center" mt={5}>
            <CircularProgress />
          </Box>
        ) : (
          <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
            <Table>
              <TableHead sx={{ bgcolor: 'background.paper' }}>
                <TableRow>
                  <TableCell><strong>Fecha</strong></TableCell>
                  <TableCell><strong>Tipo</strong></TableCell>
                  <TableCell align="right"><strong>Monto</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(!datos?.movimientos || datos.movimientos.length === 0) ? (
                  <TableRow>
                    <TableCell colSpan={3} align="center">
                      No hay movimientos registrados.
                    </TableCell>
                  </TableRow>
                ) : (
                  datos.movimientos.map((row, index) => (
                    <TableRow key={index} hover>
                      <TableCell>{new Date(row.fechaEvento).toLocaleString()}</TableCell>
                      <TableCell>
                        <Chip 
                          label={row.tipoMovimiento.toUpperCase()} 
                          color={row.tipoMovimiento.toLowerCase().includes('deposito') ? 'success' : 'error'} 
                          size="small" 
                        />
                      </TableCell>
                      <TableCell align="right" sx={{ fontWeight: 'bold' }}>
                        ${row.monto.toLocaleString('es-MX', { minimumFractionDigits: 2 })}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Container>
    </Box>
  );
}

export default Movimientos;
