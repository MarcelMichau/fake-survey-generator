import React, { useState } from 'react';
import Auth from './Auth';

const GetSurvey = ({ surveyId, onUpdateSurveyId, onFetch, surveyDetail }) => {
	return (
		<div>
			<form className="pure-form">
				{' '}
				<label>
					Survey ID
					<div>
						<input
							style={{ margin: '1em auto', color: 'black' }}
							type="text"
							value={surveyId}
							onChange={e => onUpdateSurveyId(e.target.value)}
						/>
					</div>
				</label>
			</form>

			<button className="pure-button pure-button-primary" onClick={onFetch}>
				Fetch
			</button>

			{surveyDetail && (
				<div
					style={{ border: '2px solid white', margin: '1em', padding: '1em' }}
				>
					<h3>
						This Survey Asked{' '}
						<em>
							{surveyDetail.numberOfRespondents} {surveyDetail.respondentType}
						</em>{' '}
						the Question:
					</h3>
					<h2>
						<em>{surveyDetail.topic}</em>
					</h2>
					<h3>And the results were:</h3>

					<ol style={{ display: 'inline-block', textAlign: 'left' }}>
						{surveyDetail.options
							.sort((x, y) => y.numberOfVotes - x.numberOfVotes)
							.map((option, index) => (
								<li key={option.id}>
									{index === 0 ? (
										<strong style={{ color: 'rgb(28, 184, 65)' }}>
											{option.optionText} - {option.numberOfVotes} votes
										</strong>
									) : (
										<span>
											{option.optionText} - {option.numberOfVotes} votes
										</span>
									)}
								</li>
							))}
					</ol>
				</div>
			)}
		</div>
	);
};

const CreateSurvey = ({ onCreateSurvey }) => {
	const [respondentType, setRespondentType] = useState('');
	const [topic, setTopic] = useState('');
	const [numberOfRespondents, setNumberOfRespondents] = useState(0);
	const [options, setOptions] = useState([{ id: 1, optionText: '' }]);

	const updateOption = (optionId, optionText) => {
		setOptions(
			options.map(option => {
				if (option.id !== optionId) return option;

				return {
					...option,
					optionText
				};
			})
		);
	};

	const onSubmit = e => {
		e.preventDefault();
		const surveyCommand = {
			surveyTopic: topic,
			numberOfRespondents,
			respondentType,
			surveyOptions: options.map(option => ({ optionText: option.optionText }))
		};

		onCreateSurvey(surveyCommand);

		setRespondentType('');
		setTopic('');
		setNumberOfRespondents(0);
		setOptions([{ id: 1, optionText: '' }]);
	};

	return (
		<div>
			<form onSubmit={onSubmit} className="pure-form">
				<label>
					Target Audience
					<div>
						<input
							style={{ margin: '1em auto', color: 'black' }}
							type="text"
							value={respondentType}
							onChange={e => setRespondentType(e.target.value)}
						/>
					</div>
				</label>
				<label>
					Question
					<div>
						<input
							style={{ margin: '1em auto', color: 'black' }}
							type="text"
							value={topic}
							onChange={e => setTopic(e.target.value)}
						/>
					</div>
				</label>
				<label>
					Number of Respondents
					<div>
						<input
							style={{ margin: '1em auto', color: 'black' }}
							type="number"
							value={numberOfRespondents}
							onChange={e => setNumberOfRespondents(Number(e.target.value))}
						/>
					</div>
				</label>

				<span>Options</span>

				{options.map(option => (
					<div key={option.id}>
						<label>
							#{option.id}
							<div>
								<input
									style={{ margin: '1em auto', color: 'black' }}
									type="text"
									value={option.optionText}
									onChange={e => updateOption(option.id, e.target.value)}
								/>
							</div>
						</label>
					</div>
				))}

				<div>
					<button
						style={{
							margin: '1em',
							background: 'rgb(28, 184, 65)',
							color: 'white'
						}}
						className="pure-button"
						type="button"
						onClick={() =>
							setOptions([
								...options,
								{ id: options.length + 1, optionText: '' }
							])
						}
					>
						+ Add Option
					</button>
				</div>

				<button className="pure-button pure-button-primary" type="submit">
					Create Survey
				</button>
			</form>
		</div>
	);
};

function App() {
	const [surveyId, setSurveyId] = useState(1);
	const [surveyDetail, setSurveyDetail] = useState(null);

	const fetchSurvey = async surveyId => {
		const response = await fetch(`/api/survey/${surveyId}`);
		const data = await response.json();

		setSurveyDetail(data);
	};

	const createSurvey = async surveyCommand => {
		const response = await fetch(`/api/survey`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(surveyCommand)
		});
		const data = await response.json();

		setSurveyId(data.id);
		setSurveyDetail(data);
	};

	return (
		<div>
			<div style={{ display: 'flex', justifyContent: 'flex-end' }}>
				<Auth></Auth>
			</div>
			<div style={{ textAlign: 'center' }}>
				<h1>Fake Survey Generator</h1>

				<h2>Get Survey</h2>
				<GetSurvey
					surveyId={surveyId}
					onUpdateSurveyId={value => setSurveyId(value)}
					onFetch={() => fetchSurvey(surveyId)}
					surveyDetail={surveyDetail}
				/>
				<div style={{ margin: '2em' }}>---- ¯\_(ツ)_/¯ ----</div>
				<h2>Create Survey</h2>
				<CreateSurvey onCreateSurvey={createSurvey} />
			</div>
		</div>
	);
}

export default App;
