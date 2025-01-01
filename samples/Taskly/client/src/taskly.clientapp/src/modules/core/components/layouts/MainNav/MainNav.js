import styles from "./MainNav.module.scss";
import { Link } from "react-router-dom";

const MainNav = () => {
  return (
    <nav className={styles.mainNavContent}>
      <ul>
        <li>
          <Link to="/">Home</Link>
        </li>
        <li>
          <Link to="/register">Register</Link>
        </li>
        <li>
          <Link to="/login">Login</Link>
        </li>
      </ul>
    </nav>
  );
};
export default MainNav;
