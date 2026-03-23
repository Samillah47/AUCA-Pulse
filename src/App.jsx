import { AppRouter } from './routes/routes';
import { useTheme } from './store/uiStore';

/**
 * The main App component.
 * It sets up the theme and renders the application's router.
 */
function App() {
  // This custom hook applies the current theme (light/dark) to the HTML element
  useTheme();

  // AppRouter contains all the logic for which page to display
  return <AppRouter />;
}

export default App;
