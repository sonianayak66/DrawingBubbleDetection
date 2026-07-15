import React from "react";
import {
  Routes,
  Route,
  Navigate,
  useNavigate,
  useLocation,
} from "react-router-dom";
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  ToggleButtonGroup,
  ToggleButton,
} from "@mui/material";
import {
  Brightness4,
  Brightness7,
  Assignment,
  Description,
  MoveToInbox,
  Home,
  ArrowBack
} from "@mui/icons-material";
import { useTheme } from "../context/ThemeContext";
import { usePermissions } from "../context/PermissionsContext";
import ModuleRouter from "./ModuleRouter";

const ThemeToggle = () => {
  const { isDarkMode, toggleTheme } = useTheme();

  return (
    <IconButton color="inherit" onClick={toggleTheme}>
      {isDarkMode ? <Brightness7 /> : <Brightness4 />}
    </IconButton>
  );
};

const ModuleSwitcher = ({ activeModule }) => {
  const { hasPermission } = usePermissions();
  const navigate = useNavigate();

  const modules = [
    {
      id: "taskplanner",
      label: "Task Planner",
      icon: <Assignment fontSize="small" />,
      permissions: ["TaskPlanner_Tasks_Read"],
      path: "/taskplanner", // This will be relative to /v3modules/
    },
    {
      id: "ion",
      label: "Outgoing ION",
      icon: <Description fontSize="small" />,
      permissions: ["ION_View"],
      path: "/ion", // This will be relative to /v3modules/
    },
    {
      id: "inward-ion",
      label: "Inward ION",
      icon: <MoveToInbox fontSize="small" />,
      permissions: ["ION_Inward_View"],
      path: "/inward-ion",
    },
  ];

  const availableModules = modules.filter((module) =>
    module.permissions.some((permission) => hasPermission(permission)),
  );

  if (availableModules.length <= 1) {
    return null; // Don't show switcher if only one module is available
  }

  const handleModuleChange = (event, newModule) => {
    if (newModule !== null) {
      const module = modules.find((m) => m.id === newModule);
      if (module) {
        navigate(module.path);
      }
    }
  };

  return (
    <ToggleButtonGroup
      value={activeModule}
      exclusive
      onChange={handleModuleChange}
      size="small"
      sx={{
        mr: 2,
        "& .MuiToggleButton-root": {
          color: "text.primary",
          border: "1px solid",
          borderColor: "divider",
          "&.Mui-selected": {
            bgcolor: "primary.main",
            color: "primary.contrastText",
            "&:hover": {
              bgcolor: "primary.dark",
            },
          },
        },
      }}
    >
      {availableModules.map((module) => (
        <ToggleButton key={module.id} value={module.id}>
          {module.icon}
          <Typography variant="body2" sx={{ ml: 1 }}>
            {module.label}
          </Typography>
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  );
};

const AppShell = () => {
  const location = useLocation();

  // Determine active module from URL (now relative to /v3modules/)
  const activeModule = location.pathname.startsWith("/inward-ion")
    ? "inward-ion"
    : location.pathname.startsWith("/ion")
    ? "ion"
    : "taskplanner";

  return (
    <Box sx={{ display: "flex", flexDirection: "column", height: "100vh" }}>
      {/* Global Top Bar */}
      <AppBar
        position="static"
        elevation={0}
        sx={{
          borderBottom: "1px solid",
          borderColor: "divider",
          bgcolor: "background.paper",
          color: "text.primary",
          zIndex: (theme) => theme.zIndex.drawer + 1,
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            href="/Home/Index"
            sx={{ mr: 1 }}
            title="Back to Home"
          >
            <>
              <ArrowBack sx={{ mr: 0.5, fontSize: 20 }} />
              <Home />
            </>
          </IconButton>

          <Typography
            variant="h6"
            component="div"
            sx={{ flexGrow: 1, fontWeight: 600 }}
          >
            STFE-V2
          </Typography>

          <ModuleSwitcher activeModule={activeModule} />

          <ThemeToggle />
        </Toolbar>
      </AppBar>

      {/* Module Content with Routes */}
      <Box sx={{ flexGrow: 1, overflow: "hidden" }}>
        <Routes>
          <Route
            path="/taskplanner/*"
            element={<ModuleRouter activeModule="taskplanner" />}
          />
          <Route path="/ion/*" element={<ModuleRouter activeModule="ion" />} />
          <Route path="/inward-ion/*" element={<ModuleRouter activeModule="inward-ion" />} />
          <Route path="/" element={<Navigate to="/taskplanner" replace />} />
          <Route path="*" element={<Navigate to="/taskplanner" replace />} />
        </Routes>
      </Box>
    </Box>
  );
};

export default AppShell;
