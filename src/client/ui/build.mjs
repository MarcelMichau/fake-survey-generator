import nbgv from "nerdbank-gitversioning";
import process from "process";
import { execSync } from "child_process";

(async () => {
  const env = Object.create(process.env);
  const version = await nbgv.getVersion();
  env.REACT_APP_VERSION = version.semVer1;
  execSync("react-scripts build", { env: env, stdio: "inherit" });
})();
