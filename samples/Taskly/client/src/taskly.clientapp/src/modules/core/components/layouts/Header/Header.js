import styles from './Header.module.css'
import MainNav from '../MainNav/MainNav';

const Header = () => {
  return (
    <div className={styles.headerContent}>
          <nav className={styles.mainNav}>
            <MainNav/>
          </nav>
    </div>
  );
};

export default Header;
