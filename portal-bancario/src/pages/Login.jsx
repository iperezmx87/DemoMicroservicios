import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Container, Card, CardContent, Typography, TextField, 
  Button, Box, Alert, CircularProgress 
} from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';

function Login() {
  const [usuario, setUsuario] = useState('');
  const [secreto, setSecreto] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    
    if (!usuario || !secreto) {
      setError('Por favor, ingresa tu usuario y contraseña.');
      return;
    }

    setLoading(true);
    try {
      // El microservicio UsuariosCuentas corre en el puerto 7046 (https)
      const response = await fetch('https://localhost:7046/api/CuentaUsuario/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ usuario, secreto })
      });

      const data = await response.json();

      if (response.ok && data.success) {
        // Guardar el token de forma segura en localStorage
        localStorage.setItem('jwt_token', data.token);
        // Redirigir al dashboard
        navigate('/dashboard');
      } else {
        setError(data.mensaje || 'Credenciales incorrectas');
      }
    } catch (err) {
      setError('Error al conectar con el servidor. Verifica que los servicios estén encendidos.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xs" sx={{ height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <Card sx={{ width: '100%', p: 2 }}>
        <CardContent>
          <Box display="flex" flexDirection="column" alignItems="center" mb={3}>
            <Box sx={{ 
              bgcolor: 'primary.main', 
              color: 'primary.contrastText', 
              p: 1.5, 
              borderRadius: '50%', 
              mb: 1 
            }}>
              <LockOutlinedIcon fontSize="large" />
            </Box>
            <Typography variant="h5" fontWeight="bold">
              Iniciar Sesión
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Portal Bancario
            </Typography>
          </Box>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <form onSubmit={handleLogin}>
            <TextField
              label="Usuario"
              variant="outlined"
              fullWidth
              margin="normal"
              value={usuario}
              onChange={(e) => setUsuario(e.target.value)}
              disabled={loading}
            />
            <TextField
              label="Contraseña"
              type="password"
              variant="outlined"
              fullWidth
              margin="normal"
              value={secreto}
              onChange={(e) => setSecreto(e.target.value)}
              disabled={loading}
            />
            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              size="large"
              sx={{ mt: 3, mb: 2 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Acceder'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </Container>
  );
}

export default Login;
