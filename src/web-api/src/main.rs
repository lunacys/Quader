/*
 * Copyright (c) MonkeFix. Licensed under the MIT License.
 * See the LICENCE file in the repository root for full licence text.
 */

use dotenvy::dotenv;

#[actix_web::main]
async fn main() -> anyhow::Result<()> {
    dotenv().ok();
    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));

    Ok(())
}
