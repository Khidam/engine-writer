# Build e release do Prince Titan

## Caminho recomendado: GitHub gera o EXE

O workflow `.github/workflows/prince-titan-unity-release.yml` usa Unity 2022.3.62f1 e GameCI v4 para gerar Windows x64.

### Ativação única da licença Personal

1. No Unity Hub do seu PC, confirme uma licença Personal em **Preferences → Licenses → Add → Get a free personal license**.
2. Abra `C:\ProgramData\Unity\Unity_lic.ulf` com o Bloco de Notas.
3. No GitHub, abra este repositório e vá a **Settings → Secrets and variables → Actions**.
4. Crie `UNITY_LICENSE` com todo o conteúdo do `.ulf`.
5. Crie `UNITY_EMAIL` e `UNITY_PASSWORD` com os dados da mesma conta Unity.
6. Vá a **Actions → Prince Titan Unity Windows Release → Run workflow**.

Quando termina, o workflow publica `PrinceTitan-Windows-x64.zip` em Releases e também o mantém como artifact da execução.

## Build local opcional

Se algum dia quiser testar pelo editor:

1. Abra a raiz do repositório no Unity Hub com Unity 2022.3.62f1.
2. Use **Prince Titan → Create & Open Runtime Scene** e aperte Play.
3. Para compilar, use **Prince Titan → Build Windows x64**.

O aplicativo em si não exige Unity instalado depois que o ZIP da release foi criado.
