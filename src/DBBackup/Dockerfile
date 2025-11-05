FROM mcr.microsoft.com/mssql/server:2022-latest

ENV SA_PASSWORD=OhLord
ENV ACCEPT_EULA=Y

USER root
RUN mkdir -p /var/opt/mssql/backup /var/opt/mssql/scripts

COPY *.bak /var/opt/mssql/backup 
COPY restore.sql /var/opt/mssql/scripts/restore.sql

# --Db restored during image build --
USER mssql
RUN (/opt/mssql/bin/sqlservr & sleep 20 && /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P OhLord -N -C -i /var/opt/mssql/scripts/restore.sql && pkill sqlservr) || exit 1

CMD ["/opt/mssql/bin/sqlservr"]