import styles from "./Logo.module.scss";
import logo from "./../../../../assets/logo-desktop.png";

const Logo = () => {
  return (
    <div className={styles.logoContent}>
      <img src={logo} className={styles.logo} alt="logotype" />
    </div>
  );
};
export default Logo;
